using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XCV.Data;
using XCV.Entities;

namespace XCV.Services
{
    /// <summary>
    /// imports Database json and parses it into the different Experience Tables
    /// </summary>
    public class FillExperienceTable
    {
        [Inject] private IExperienceService ExperienceService { get; set; }
        private delegate Task JsonCreeperCallback(string nextElement, string category);
        private readonly string _pathTo =  Path.Combine(".", "Files", "datenbasis.json");
        private JObject? _dataBase;
        
        /// <summary>
        /// initialize ExperienceService
        /// loads json into JObject
        /// </summary>
        /// <param name="experienceService"></param>
        public FillExperienceTable(IExperienceService experienceService)
        {
            ExperienceService = experienceService;
        }

        /// <summary>
        /// inserts Experience in corresponding database table
        /// </summary>
        public async Task Fill()
        {
            _dataBase = await JObject.LoadAsync(new JsonTextReader(new StreamReader(_pathTo)));
            await Fill(_dataBase);
        }

        /// <summary>
        /// inserts Experience in corresponding database table
        /// </summary>
        public async Task Fill(JObject json)
        {
            foreach (var (key, value) in json)
            {
                if (value == null) return;
                switch (key)
                { 
                    case "fields":
                    {
                        await CreepJson(value, async (element, _) => await ExperienceService.UpdateExperience(new Field(element)));
                        break;
                    }
                    case "roles":
                    {
                        await CreepJson(value, async (element, _) => await ExperienceService.UpdateExperience(new Role(element)));
                        break;
                    }
                    case "languages":
                    {
                        await CreepJson(value, async (element, _) => await ExperienceService.UpdateExperience(new Language(element)));
                        break;
                    }
                    case "Softskills":
                    {
                        await CreepJson(value, async (element, _) => await ExperienceService.UpdateExperience(new SoftSkill(element)));
                        break;
                    }
                    case "skills":
                    {
                        await CreepJson(value, async (element, category) => await ExperienceService.UpdateExperience(new HardSkill(element, category)));
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// creeps throw each Experience category
        /// </summary>
        /// <param name="next"></param>
        /// <param name="callback"></param>
        /// <param name="category"></param>
        private static async Task CreepJson(JToken next, JsonCreeperCallback callback, string category="")
        {
            if (!next.HasValues)
            {
                await callback(next.ToString(), category);
            } 
            else switch (next)
            {
                case JArray array:
                {
                    foreach (var value in array)
                    {
                        await CreepJson(value, callback, category);
                    }

                    break;
                }
                case JObject jObject:
                {
                    if (category.Length != 0)
                        category += "$$";
            
                    foreach (var (key, value) in jObject)
                    {
                        if (value != null) await CreepJson(value, callback, category + key);
                    }
                    break;
                }
            }
        }
    }
}