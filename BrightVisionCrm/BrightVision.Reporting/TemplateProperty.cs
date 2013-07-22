using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.ComponentModel;
using System.Reflection;
using System.Collections;
namespace BrightVision.Reporting.Template
{
    [Serializable()]
    public class TemplateProperty
    {

        public bool IsFooterVisible { get; set; }
        public bool IsPageNumberVisible { get; set; }
        public bool IsEmptyDynamicValueVisible { get; set; }
        public List<TemplateDynamicData> DynamicProperty { get; set; }
        public DataTable StatisticsDataSource { get; set; }
    }

    public class TemplateDynamicData
    {
        public string CategoryName { get; set; }
        public TemplateDynamicType Type { get; set; }
        public string TextField { get; set; }
        public StatisticsTemplate Statistics { get; set; }
        public int Order { get; set; }
        public bool IsVisible { get; set; }
        public TextField Text { get; set; }

    }

    public enum TemplateDynamicType
    {
        PageBreak,
        TextField,
        Statistics
    }

    public class StatisticsTemplate
    {
        public string ColumnName { get; set; }
        public string ChartTitle { get; set; }
        public bool IsTotalCountVisible { get; set; }
        public bool IsNullCategoryVisible { get; set; }
        public List<KeyValuePair<string, int>> Data { get; set; }
        public PercentageValueSortingOption PercentageValueSortBy { get; set; }
        public GraphSize GraphSize { get; set; }
    }

    public class TextField
    {
        public string Text { get; set; }
        public TextFieldFontSize Size { get; set; }
        public TextFieldFontStyle Style { get; set; }
    }

    public enum TextFieldFontSize
    {
        [EnumDescriptionAttribute("Small")]
        Small = 0,
        [EnumDescriptionAttribute("Normal")]
        Normal = 1,
        [EnumDescriptionAttribute("Large")]
        Large = 2
    }
    public enum GraphSize
    {
        [EnumDescriptionAttribute("Small")]
        Small = 0,
        [EnumDescriptionAttribute("Normal")]
        Normal = 1,
        [EnumDescriptionAttribute("Large")]
        Large = 2
    }

    public enum TextFieldFontStyle
    {
        [EnumDescriptionAttribute("None")]
        None,
        [EnumDescriptionAttribute("Bold")]
        Bold,
        [EnumDescriptionAttribute("Italic")]
        Italic,
        [EnumDescriptionAttribute("Bold and Italic")]
        BoldItalic
    }
    public enum PercentageValueSortingOption
    {
        [EnumDescriptionAttribute("Percentage - Decending")]
        PercentageDescending,
        [EnumDescriptionAttribute("Percentage - Ascending")]
        PercentageAscending,
        [EnumDescriptionAttribute("Value - Descending")]
        ValueDescending,
        [EnumDescriptionAttribute("Value - Ascending")]
        ValueAscending
    }
    public sealed class EnumDescriptionAttribute : Attribute
    {
        private string description;

        /// <summary>
        /// Gets the description stored in this attribute.
        /// </summary>
        /// <value>The description stored in the attribute.</value>
        public string Description
        {
            get
            {
                return this.description;
            }
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="EnumDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="description">The description to store in this attribute.
        /// </param>
        public EnumDescriptionAttribute(string description)
            : base()
        {
            this.description = description;
        }
    }

    public static class EnumHelper
    {

        public static string GetDescription(this Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            string description = value.ToString();
            FieldInfo fieldInfo = value.GetType().GetField(description);
            EnumDescriptionAttribute[] attributes =
               (EnumDescriptionAttribute[])
             fieldInfo.GetCustomAttributes(typeof(EnumDescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                description = attributes[0].Description;
            }
            return description;
        }


        public static IList ToEnumList(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            ArrayList list = new ArrayList();
            Array enumValues = Enum.GetValues(type);

            foreach (Enum value in enumValues)
            {
                list.Add(new KeyValuePair<Enum, string>(value, GetDescription(value)));
            }

            return list;
        }
    }

}

