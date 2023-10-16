using MVR.FileManagementSecure;
using SimpleJSON;
using System;
using System.Text;

namespace mmd2timeline
{
    /// <summary>
    /// 继承的JSONClass抽象类
    /// </summary>
    /// <remarks>主要提供文件保存/加载相关的便利方法</remarks>
    internal abstract class MSJSONClass : JSONClass
    {
        private string _SaveFileName;
        /// <summary>
        /// 获取或设置保存文件的名称
        /// </summary>
        protected virtual string SaveFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_SaveFileName))
                {
                    throw new Exception("No value is set for the SaveFileName variable.");
                }

                return _SaveFileName;
            }
            set
            {
                _SaveFileName = value;
            }
        }

        /// <summary>
        /// 加载数据到当前对象
        /// </summary>
        public virtual void Load()
        {
            this.Load<MSJSONClass>();
        }

        /// <summary>
        /// 从文件加载数据到当前对象
        /// </summary>
        /// <param name="filename"></param>
        public virtual void Load(string filename)
        {
            this.Load<MSJSONClass>(filename);
        }

        /// <summary>
        /// 根据文件名成加载数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <returns></returns>
        public virtual T Load<T>(string filename) where T : MSJSONClass
        {
            this.SaveFileName = filename;

            try
            {
                if (FileManagerSecure.FileExists(filename))
                {
                    var json = SuperController.singleton.LoadJSON(filename);

                    return this.LoadFromJSON<T>(json.AsObject);
                }
                else return null;
            }
            catch { }

            return this as T;
        }

        /// <summary>
        /// 从文件加载数据
        /// </summary>
        /// <returns></returns>
        public virtual T Load<T>() where T : MSJSONClass
        {
            return this.Load<T>(SaveFileName);
        }

        /// <summary>
        /// 从JSONClass对象获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public virtual T LoadFromJSON<T>(JSONClass json) where T : MSJSONClass
        {
            if (json != null)
            {
                foreach (var key in json.Keys)
                {
                    this[key] = json[key];
                }
            }

            return this as T;
        }

        /// <summary>
        /// 将数据保存到文件
        /// </summary>
        public virtual void Save()
        {
            this.SaveTo(this.SaveFileName);
        }

        /// <summary>
        /// 将数据保存到指定文件
        /// </summary>
        /// <param name="filename"></param>
        public virtual void Save(string filename)
        {
            this.SaveFileName = filename;
            this.SaveTo(filename);
        }

        /// <summary>
        /// 保存到指定文件
        /// </summary>
        /// <param name="filename"></param>
        public virtual void SaveTo(string filename)
        {
            try
            {
                FileManagerSecure.CreateDirectory(FileManagerSecure.GetDirectoryName(filename));
                SuperController.singleton.SaveJSON(this, filename);
            }
            catch (Exception e)
            {
                LogUtil.Debug(e, $"SaveTo::{filename}");
            }
        }

        #region ToString相关的处理
        /// <summary>
        /// 转换未字符串时的操作
        /// </summary>
        protected abstract void BeforeToString();

        /// <summary>
        /// 将JSON对象转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            BeforeToString();
            return base.ToString();
        }

        /// <summary>
        /// 将对象转换为stringbuilder
        /// </summary>
        /// <param name="aPrefix"></param>
        /// <param name="sb"></param>
        public override void ToString(string aPrefix, StringBuilder sb)
        {
            BeforeToString();
            base.ToString(aPrefix, sb);
        }

        /// <summary>
        /// 将对象转换为字符串
        /// </summary>
        /// <param name="aPrefix"></param>
        /// <returns></returns>
        public override string ToString(string aPrefix)
        {
            BeforeToString();
            return base.ToString(aPrefix);
        }

        #endregion
    }
}
