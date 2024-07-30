using System;
using UnityEngine;

namespace cngraphi.gassets.common
{
    /// <summary>
    /// ��ϢԪ���
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)] // �ֶΡ�ö��
    public class InfoPropAttri : PropertyAttribute
    {
        /// <summary>
        /// ����
        /// </summary>
        public DomainType Type { get; set; } = DomainType.General;

        /// <summary>
        /// �ֶ�������Ϣ
        /// </summary>
        public string Desc { get; set; } = "";

        /// <summary>
        /// �ֶΰ���˵��
        /// </summary>
        public string Help { get; set; } = "";

        /// <summary>
        /// �ֶΰ���˵���߶�
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