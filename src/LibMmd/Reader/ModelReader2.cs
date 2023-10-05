//using System.IO;
using LibMMD.Model;
using MVR.FileManagementSecure;
using mmd2timeline;
using LibMMD.Util;
namespace LibMMD.Reader
{
    public abstract class ModelReader2
    {
        
        public MmdModel Read(string path)
        {
            byte[] bytes = null;
            if (path == null)
            {
                if (FileManagerSecure.FileExists(Settings.pmxPath))
                    bytes = FileManagerSecure.ReadAllBytes(Settings.pmxPath);
                else
                    bytes = FileManagerSecure.ReadAllBytes(Settings.varPmxPath);
            }
            else
            {
                bytes = FileManagerSecure.ReadAllBytes(path);
            }


            var binaryReader = new BufferBinaryReader(bytes);
            return Read(binaryReader);
        }

        public abstract MmdModel Read(BufferBinaryReader reader);
        
        public static MmdModel LoadMmdModel(string path)
        {
            //var fileExt = new FileInfo(path).Extension.ToLower();
            //if (".pmd".Equals(fileExt))
            //{
            //    return new PmdReader().Read(path, config);
            //}
            //if (".pmx".Equals(fileExt))
                return new PmxReader2().Read(path);
        }

    }
}