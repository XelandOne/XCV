using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XCV.Entities;

namespace XCV.Pages
{
    public partial class ExperienceDatabase
    {
        /// <summary>
        /// A Path down the skill tree, where all nodes in the Path are expanded
        /// </summary>
        [Parameter]
        public string? ExpandPath { get; set; }
        private TreeElement _root = new() {Name = "root"};
        private TreeElement _current = new();
        private object? _selected;
        private readonly ExperienceModel _experienceModel = new();
        private bool _initialized;
        private string _searchRequest = "";

        private bool _jsonToLarge;
        private const double MaxFileSizeMb = 50;

        private bool _uploadException;
        private bool _dataAlreadyLoaded;
        private bool _showEditData;
        private bool _showCreateData;
        private bool _showError;
        private bool _showCommitDelete;
        private bool _showNotFound;


        /// <inheritdoc />
        protected override async Task OnInitializedAsync()
        {
            await _experienceManager.Load();
            if (_initialized) return;
            _root = InitTree();
            if (ExpandPath != null) ExpandTree(ExpandPath);
            _initialized = true;
        }

        private void ExpandTree(string expandPath)
        {
            var split = expandPath.Split("%20");
            var oldElement = _root;
            foreach(var s in split)
            {
                var element = oldElement.Children.Find(e => e.Name == s);
                if(element == null) break;
                element.Expanded = true;
                oldElement = element;
            }
        }

        private TreeElement InitTree()
        {
            HashSet<string> categories = new();
            TreeElement root = new TreeElement() {Name = "root"};

            TreeElement hardSkillRoot = new TreeElement() {Name = "Hard Skills", Parent = root};
            IEnumerable<HardSkill> hardSkills = _experienceManager.HardSkills;
            foreach (var hardSkill in hardSkills)
            {
                var split = hardSkill.HardSkillCategory.Split("$$");
                var added = categories.Add(split[0]);
                if (added)
                {
                    var node = new TreeElement() {Name = split[0], Parent = hardSkillRoot, Origin = hardSkill};
                    hardSkillRoot.Children.Add(node);
                    for (var i = 1; i < split.Length; i++)
                    {
                        var nextNode = new TreeElement() {Name = split[i], Parent = node, Origin = hardSkill};
                        node.Children.Add(nextNode);
                        node = nextNode;
                    }

                    node.Children.Add(new TreeElement() {Name = hardSkill.Name, Parent = node, Origin = hardSkill});
                }
                else
                {
                    var node = hardSkillRoot;
                    foreach (var t in split)
                    {
                        if (node != null && node.Children.Exists(element => element.Name == t))
                        {
                            node = node.Children.Find(element => element.Name == t);
                            continue;
                        }

                        if (node == null) continue;
                        var nextNode = new TreeElement() {Name = t, Parent = node, Origin = hardSkill};
                        node.Children.Add(nextNode);
                        node = nextNode;
                    }

                    node?.Children.Add(new TreeElement() {Name = hardSkill.Name, Parent = node, Origin = hardSkill});
                }
            }

            TreeElement softSkillRoot = new TreeElement() {Name = "Soft Skills", Parent = root};
            TreeElement fieldsRoot = new TreeElement() {Name = "Branchen", Parent = root};
            TreeElement rolesRoot = new TreeElement() {Name = "Rollen", Parent = root};
            TreeElement languagesRoot = new TreeElement() {Name = "Sprachen", Parent = root};
            _experienceManager.SoftSkills.ForEach(s =>
                softSkillRoot.Children.Add(new TreeElement() {Name = s.Name, Parent = softSkillRoot, Origin = s}));
            _experienceManager.Fields.ForEach(s =>
                fieldsRoot.Children.Add(new TreeElement() {Name = s.Name, Parent = fieldsRoot, Origin = s}));
            _experienceManager.Roles.ForEach(s =>
                rolesRoot.Children.Add(new TreeElement() {Name = s.Name, Parent = rolesRoot, Origin = s}));
            _experienceManager.Languages.ForEach(
                s => languagesRoot.Children.Add(new TreeElement() {Name = s.Name, Parent = languagesRoot, Origin = s}));
            root.Children.Add(hardSkillRoot);
            root.Children.Add(softSkillRoot);
            root.Children.Add(fieldsRoot);
            root.Children.Add(languagesRoot);
            root.Children.Add(rolesRoot);

            return root;
        }

        private void Toggle(object obj)
        {
            if (obj is not TreeElement node) return;
            node.Expanded = !node.Expanded;
        }

        private void OnDelete(TreeElement? args)
        {
            if (args == null) return;
            _current = args;
            _showCommitDelete = true;
        }

        private async void OnCommitDelete()
        {
            _current.Parent?.Children.Remove(_current);
            _showCommitDelete = false;
            if (_current.Origin != null) await _experienceManager.DeleteExperience(_current.Origin);
        }

        private async void ExpSubmitHandler()
        {
            _showEditData = false;
            //normalize name
            var name = _experienceModel.Name.Trim();
            if (!name.Equals(_current.Name, StringComparison.OrdinalIgnoreCase))
            {
                //check if name is taken
                if (_experienceManager.HardSkills.Exists(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ||
                    _experienceManager.SoftSkills.Exists(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ||
                    _experienceManager.Languages.Exists(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ||
                    _experienceManager.Roles.Exists(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ||
                    _experienceManager.Fields.Exists(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    _showError = true;
                    _current = new TreeElement();
                    _experienceModel.Name = "";
                    return;
                }
            }
            if (_current.Origin == null) return;

            _current.Origin.Name = name;
            _current.Name = name;
            await _experienceManager.UpdateExperience(_current.Origin);
            _experienceModel.Name = "";
        }

        private async void NewExpSubmitHandler()
        {
            TreeElement? treeNode = null;
            Experience? exp = null;
            string expandPath = "";
            
            _showCreateData = false;
            //normalize name
            var name = _experienceModel.Name.Trim();

            //check if name is taken
            if (_experienceManager.HardSkills.Exists(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ||
                _experienceManager.SoftSkills.Exists(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ||
                _experienceManager.Languages.Exists(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ||
                _experienceManager.Roles.Exists(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ||
                _experienceManager.Fields.Exists(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                _showError = true;
                _current = new TreeElement();
                _experienceModel.Name = "";
                return;
            }
            
            //insert if hardskill
            if (_current.Origin is HardSkill hardSkillOrigin)
            {
                var hardSkill = new HardSkill(name, hardSkillOrigin.HardSkillCategory);
                treeNode = new TreeElement() {Name = name, Parent = _current, Origin = hardSkill};
                await _experienceManager.InsertExperience(hardSkill);
                _current.Parent?.Children.Find(s => s.Name == _current.Name)?.Children.Add(treeNode);
                _current = new TreeElement();
                _experienceModel.Name = "";
                _selected = treeNode;
                //reload
                _navigationManager.NavigateTo(_navigationManager.BaseUri + "/Datenbasis/" + hardSkillOrigin.HardSkillCategory);
                return;
            }

            //check the type
            switch (_current.Name)
            {
                case "Soft Skills":
                {
                    exp = new SoftSkill(name);
                    treeNode = new TreeElement() {Name = name, Parent = _current, Origin = exp};
                    expandPath = "Soft Skills";
                    break;
                }
                case "Branchen":
                {
                    exp = new Field(name);
                    treeNode = new TreeElement() {Name = name, Parent = _current, Origin = exp};
                    expandPath = "Branche";
                    break;
                }
                case "Rollen":
                {
                    exp = new Role(name);
                    treeNode = new TreeElement() {Name = name, Parent = _current, Origin = exp};
                    expandPath = "Rollen";
                    break;
                }
                case "Sprachen":
                {
                    exp = new Language(name);
                    treeNode = new TreeElement() {Name = name, Parent = _current, Origin = exp};
                    expandPath = "Sprachen";
                    break;
                }
            }
            if(exp == null || treeNode == null) return; 
            //insert it
            await _experienceManager.InsertExperience(exp);
            _current.Parent?.Children.Find(s => s.Name == _current.Name)?.Children.Add(treeNode);
            //reset
            _current = new TreeElement();
            _experienceModel.Name = "";
            //reload
            _navigationManager.NavigateTo(_navigationManager.BaseUri + "/Datenbasis/" + expandPath);
            
        }

        private void DismissHandler()
        {
            _current = new TreeElement();
            _experienceModel.Name = "";
            _showError = false;
            _showCommitDelete = false;
            _showEditData = false;
            _showCreateData = false;
        }

        private void SearchHandler()
        {
            Experience? exp = null;
            TreeElement? element = null;
            _searchRequest = _searchRequest.Trim();
            //search request found in Hard Skill?
            if (_experienceManager.HardSkills.Exists(s =>
                s.Name.Equals(_searchRequest, StringComparison.OrdinalIgnoreCase)))
            {
                exp = _experienceManager.HardSkills.Find(s =>
                    s.Name.Equals(_searchRequest, StringComparison.OrdinalIgnoreCase));
                var split = (exp as HardSkill)?.HardSkillCategory.Split("$$");
                var category = _root.Children.Find(r => r.Name == "Hard Skills");
                foreach (var t in split)
                {
                    if (category == null) return;
                    category.Expanded = true;
                    //go down the tree
                    category = category.Children.Find(c => c.Name == t);
                }


                if (category == null) return;
                //element is now last category. now find the exp and select it
                category.Expanded = true;
                _selected = category.Children.Find(s => s.Name == exp.Name);
                return;
            }
            //search request found in Soft Skill?
            if (_experienceManager.SoftSkills.Exists(s =>
                s.Name.Equals(_searchRequest, StringComparison.OrdinalIgnoreCase)))
            {
                exp = _experienceManager.SoftSkills.Find(s =>
                    s.Name.Equals(_searchRequest, StringComparison.OrdinalIgnoreCase));
                element = _root.Children.Find(r => r.Name == "Soft Skills");
            }
            
            //search request found in Fields?
            else if (_experienceManager.Fields.Exists(s =>
                s.Name.Equals(_searchRequest, StringComparison.OrdinalIgnoreCase)))
            {
                exp = _experienceManager.Fields.Find(s =>
                    s.Name.Equals(_searchRequest, StringComparison.OrdinalIgnoreCase));
                element = _root.Children.Find(r => r.Name == "Branchen");
            }
            
            //search request found in Languages?
            else if (_experienceManager.Languages.Exists(s =>
                s.Name.Equals(_searchRequest, StringComparison.OrdinalIgnoreCase)))
            {
                exp = _experienceManager.Languages.Find(s =>
                    s.Name.Equals(_searchRequest, StringComparison.OrdinalIgnoreCase));
                element = _root.Children.Find(r => r.Name == "Sprachen");
            }
            
            //search request found in Roles?
            else if (_experienceManager.Roles.Exists(s =>
                s.Name.Equals(_searchRequest, StringComparison.OrdinalIgnoreCase)))
            {
                exp = _experienceManager.Roles.Find(s =>
                    s.Name.Equals(_searchRequest, StringComparison.OrdinalIgnoreCase));
                element = _root.Children.Find(r => r.Name == "Rollen");
            }

            if (element != null && exp != null)
            {
                element.Expanded = true;
                _selected = element.Children.Find(s => s.Name == exp.Name);
                return;
            }

            _showNotFound = true;
        }

        private void OnEdit(TreeElement current)
        {
            _current = current;
            _showEditData = true;
        }

        private void OnCreate(TreeElement current)
        {
            _current = current;
            _showCreateData = true;
        }


        private class TreeElement : IEnumerable
        {
            public TreeElement? Parent { get; set; }
            public List<TreeElement> Children { get; } = new();
            public string Name { get; set; } = "";
            public Experience? Origin { get; set; }
            public bool Expanded { get; set; }

            private IEnumerator<TreeElement> GetEnumerator()
            {
                return Children.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class ExperienceModel
        {
            private const string InvalidLengthMessage = "Der Name darf nur {1} Zeichen lang sein";

            [Required(ErrorMessage = "Bitte geben Sie einen Namen ein")]
            [StringLength(50, ErrorMessage = InvalidLengthMessage)]
            public string Name { get; set; } = null!;
        }

        private async Task OnExperienceJsonUpload(InputFileChangeEventArgs e)
        {
            if (await _experienceManager.Load())
            {
                _dataAlreadyLoaded = true;
                return;
            }


            _jsonToLarge = e.File.Size >= MaxFileSizeMb * 1000000;
            if (_jsonToLarge)
            {
                return;
            }

            using var jsonReader =
                new JsonTextReader(new StreamReader(e.File.OpenReadStream((long) MaxFileSizeMb * 1000000)));
            var json = await JObject.LoadAsync(jsonReader);

            try
            {
                if (json != null) await _fillExperienceTable.Fill(json);
            }
            catch (Exception)
            {
                _uploadException = true;
            }

            _initialized = false;
            //_root = InitTree();
            _navigationManager.NavigateTo("/Datenbasis", true);
        }
    }
}