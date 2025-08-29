using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using ClientAgree.Models;
using System.Text.Json;

namespace ClientAgree.Utils
{
    internal static class DummyDataProvider
    {
        public static List<Team> GetTeams()
        {
            var assembly = typeof(DummyDataProvider).GetTypeInfo().Assembly;

            using var stream = assembly.GetManifestResourceStream("ClientAgree.teams.json");
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();

            return JsonSerializer.Deserialize<List<Team>>(json);
        }
    }
}
