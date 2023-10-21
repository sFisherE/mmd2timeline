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
                if (FileManagerSecure.FileExists(Config.pmxPath))
                    bytes = FileManagerSecure.ReadAllBytes(Config.pmxPath);
                else
                    bytes = FileManagerSecure.ReadAllBytes(Config.varPmxPath);
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
            return new PmxReader2().Read(path);
        }

    }
}