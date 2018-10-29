using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MSCLoader;
using UnityEngine;

namespace CinderBlocks
{
    public class CinderBlocksSaveData
    {
        public List<Vector3> Pos;
        public List<Quaternion> Rot;

        public CinderBlocksSaveData()
        {
            Pos = new List<Vector3>();
            Rot = new List<Quaternion>();
        }

        public CinderBlocksSaveData(List<Vector3> pos, List<Quaternion> rot)
        {
            this.Pos = pos;
            this.Rot = rot;
        }

        public static string SavePath;
        public static bool saveExists = false;

        public static void Serialize<T>(T data, string path)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                XmlWriter writer = XmlWriter.Create(new StreamWriter(path));
                serializer.Serialize(writer, data);
                writer.Close();
                saveExists = true;
            }
            catch (Exception ex)
            {
                ModConsole.Error(ex.ToString());
            }
        }

        public static T Deserialize<T>(string path) where T : new()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                XmlReader reader = XmlReader.Create(new StreamReader(path));
                return (T)serializer.Deserialize(reader);
            }
            catch { }
            return new T();
        }
    }
}
