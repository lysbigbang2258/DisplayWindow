// 201812284:26 PM

namespace ArrayDisplay.UI {
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// The index conver.
    /// </summary>
    [ValueConversion(typeof(float), typeof(ListViewItem))]
    public class IndexConver : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ListViewItem item = value as ListViewItem;
            ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;

            int index = listView.ItemContainerGenerator.IndexFromContainer(item);
            string indexStr = string.Format("通道{0}时分{1}B", 1 + (index % 8), 1 + (index / 8));
            return indexStr;
        }

        /// <summary>
        /// The convert back.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    } 
}
