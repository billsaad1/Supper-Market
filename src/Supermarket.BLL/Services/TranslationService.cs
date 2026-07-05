using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supermarket.DAL;
using Supermarket.Models.Entities;

namespace Supermarket.BLL.Services
{
    public enum Language
    {
        Arabic,
        English
    }

    public class TranslationService
    {
        private readonly LocalizationRepository _repo;
        private Dictionary<string, LocalizationResource> _resources;
        public Language CurrentLanguage { get; set; } = Language.Arabic;

        public TranslationService(string connectionString)
        {
            _repo = new LocalizationRepository(connectionString);
        }

        public async Task LoadResourcesAsync()
        {
            var data = await _repo.GetAllResourcesAsync();
            _resources = data.ToDictionary(r => r.ResourceKey, r => r);
        }

        public string Translate(string key)
        {
            if (_resources != null && _resources.TryGetValue(key, out var resource))
            {
                return CurrentLanguage == Language.Arabic ? resource.ArabicValue : resource.EnglishValue;
            }
            return key;
        }
    }
}
