﻿using NppLogGazer.Common.Repository.Exception;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NppLogGazer.Common.Repository
{
    public class XmlRepository<T> : IRepository<T>
    {
        private FileInfo file;

        public XmlRepository(FileInfo file)
        {
            this.file = file;
        }

        public List<T> GetAll()
        {
            if (!file.Exists)
            {
                return new List<T>();
            }

            using (FileStream fs = File.Open(file.FullName, FileMode.OpenOrCreate))
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<T>));
                try
                {
                    List<T> patterns = (List<T>)ser.Deserialize(fs);
                    return patterns;
                }
                catch (System.Exception exception)
                {
                    if (fs != null) fs.Close();
                    string backupFile = file.FullName + ".bak";
                    File.Copy(file.FullName, backupFile, true);
                    throw (new LoadXmlFileException(exception.Message, backupFile));
                }
            }
        }

        public List<T> GetFrom(FileInfo sourceFile)
        {
            if (!sourceFile.Exists)
            {
                return new List<T>();
            }

            using (FileStream fs = File.Open(sourceFile.FullName, FileMode.OpenOrCreate))
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<T>));
                try
                {
                    List<T> patterns = (List<T>)ser.Deserialize(fs);
                    return patterns;
                }
                catch (System.Exception exception)
                {
                    throw (new LoadXmlFileException(exception.Message));
                }
            }
        }

        public void SaveAll(List<T> patterns)
        {
            try
            {
                TextWriter writer = new StreamWriter(file.FullName);
                XmlSerializer ser = new XmlSerializer(typeof(List<T>));
                ser.Serialize(writer, patterns);
                writer.Close();
            }
            catch (System.Exception ex)
            {
                throw (new SaveXmlFileException(ex.Message));
            }
        }

        public void SaveAs(List<T> patterns, FileInfo newFile)
        {
            try
            {
                TextWriter writer = new StreamWriter(newFile.FullName);
                XmlSerializer ser = new XmlSerializer(typeof(List<T>));
                ser.Serialize(writer, patterns);
                writer.Close();
            }
            catch (System.Exception ex)
            {
                throw (new SaveXmlFileException(ex.Message));
            }
        }

    }
}
