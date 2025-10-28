using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Dictionaries
{
    public class TipoIdentificacion : IReadOnlyDictionary<int, string>
    {
        private readonly Dictionary<int, string> _data = new()
        {
            { 0,  "No asignado" },
            { 6,  "Cédula" },
            { 7,  "RUC" },
            { 8,  "Consumidor Final" },
            { 9,  "Identificación del Exterior" },
            { 10, "Pasaporte / Identificación Tributaria del Exterior" }
        };

        // Permite acceso por índice numérico
        public string this[int index] => _data.ContainsKey(index)
            ? _data[index]
            : throw new KeyNotFoundException($"No existe tipo de identificación con índice {index}.");

        // Permite acceso por nombre (opcional)
        public int this[string name]
        {
            get
            {
                foreach (var kv in _data)
                {
                    if (string.Equals(kv.Value, name, StringComparison.OrdinalIgnoreCase))
                        return kv.Key;
                }
                throw new KeyNotFoundException($"No existe tipo de identificación con nombre '{name}'.");
            }
        }

        // Implementación de IReadOnlyDictionary
        public IEnumerable<int> Keys => _data.Keys;
        public IEnumerable<string> Values => _data.Values;
        public int Count => _data.Count;

        public bool ContainsKey(int key) => _data.ContainsKey(key);
        public bool TryGetValue(int key, out string value) => _data.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<int, string>> GetEnumerator() => _data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
    }
}
