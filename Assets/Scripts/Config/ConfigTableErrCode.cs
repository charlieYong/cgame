using UnityEngine;
using System.Collections;

using ZR_MasterTools;


namespace Sgame
{
    public class TErrorCode : ConfigTableRow
    {
        public int code;
        public string info;
    }

    public class ConfigTableErrCode : ConfigTableController<ConfigTableErrCode>
    {
        // 配置初始化
        protected override void Init() {
            base.Init();
            _filename = "error_code";
            _fileList = new string[] {
            "error_code",
        };
        }

        protected override void LoadFile() {
            base.LoadFile();
            CreateConfigFileWithIndex();
        }

        ConfigTableRow ParseRow(string[] cols) {
            TErrorCode error = new TErrorCode();
            int i = 0;
            error.code = int.Parse(cols[i++]);
            error.info = cols[i++];
            return error;
        }

        /// 根据ID获取配置信息
        public TErrorCode GetInfoByID(int id) {
            ConfigTableRow data = GetDataByIndex(id.ToString(), ParseRow);
            return data as TErrorCode;
        }
    }
}