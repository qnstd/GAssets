using System;
using UnityEngine;

namespace cngraphi.gassets.common
{
    /// <summary>
    /// 信息元标记
    /// <para>作者：强辰</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)] // 字段、枚举
    public class InfoPropAttri : PropertyAttribute
    {
        /// <summary>
        /// 类型
        /// </summary>
        public DomainType Type { get; set; } = DomainType.General;

        /// <summary>
        /// 字段描述信息
        /// </summary>
        public string Desc { get; set; } = "";

        /// <summary>
        /// 字段帮助说明
        /// </summary>
        public string Help { get; set; } = "";

        /// <summary>
        /// 字段帮助说明高度
        /// </summary>
        public int HelpHeight { get; set; } = 0;


        public InfoPropAttri(string desc, DomainType type = DomainType.General, string help = "", int helpHeight = 14)
        {
            Desc = desc;
            Type = type;
            Help = help;
            HelpHeight = helpHeight;
        }
    }
}