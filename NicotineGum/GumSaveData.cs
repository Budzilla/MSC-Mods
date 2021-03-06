﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MSCLoader;
using UnityEngine;

namespace NicotineGum
{
    public class GumSaveData
    {
        public List<Vector3> Pos;
        public List<Quaternion> Rot;
        public List<int> GumCount;
        public float Overdose;

        public GumSaveData()
        {
            Pos = new List<Vector3>();
            Rot = new List<Quaternion>();
            GumCount = new List<int>();
            Overdose = 0.0f;
        }

        public GumSaveData(List<Vector3> pos, List<Quaternion> rot, List<int> gumCount, float overdose)
        {
            this.Pos = pos;
            this.Rot = rot;
            this.GumCount = gumCount;
            this.Overdose = overdose;
        }

        public static string SavePath;

        public static void Serialize<T>(T data, string path)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                XmlWriter writer = XmlWriter.Create(new StreamWriter(path));
                serializer.Serialize(writer, data);
                writer.Close();
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
