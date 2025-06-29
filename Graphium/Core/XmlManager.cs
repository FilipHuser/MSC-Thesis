using System.IO;
using System.Xml.Serialization;

namespace Graphium.Core
{
    internal static class XmlManager
    {
        private static T? LoadFromFile<T>(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (var sr = new StreamReader(filePath))
            {
                return (T?)serializer.Deserialize(sr);
            }
        }

        public static List<T> Load<T>(string folderName, string? fileName = null)
        {
            var settingsList = new List<T>();

            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Graphium", folderName);

            if (!Directory.Exists(appDataPath)) { Directory.CreateDirectory(appDataPath); }

            if (fileName != null)
            {
                string filePath = Path.Combine(appDataPath, fileName);
                if (File.Exists(filePath))
                {
                    var t = LoadFromFile<T>(filePath);
                    if (t != null)
                    settingsList.Add(t);
                }
            } else {
                var files = Directory.GetFiles(appDataPath, "*.xml");

                foreach (var file in files)
                {
                    var t = LoadFromFile<T>(file);
                    if (t != null)
                    settingsList.Add(t);
                }
            }

            return settingsList;
        }
        public static void Store<T>(string folderName, string fileName , T t , bool? overwrite = null)
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Graphium", folderName);

            if (!Directory.Exists(appDataPath)) { Directory.CreateDirectory(appDataPath); }

            string filePath = Path.Combine(appDataPath, fileName);

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (var sw = new StreamWriter(filePath , append: !(overwrite??false)))
            {
                serializer.Serialize(sw , t);
            }
        }
    }
}
