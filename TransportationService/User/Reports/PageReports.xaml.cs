﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using System.Collections;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TransportationService
{
    /// <summary>
    /// Interaction logic for PageReports.xaml
    /// </summary>
    public partial class PageReports : Page
    {
        ServiceDBEntities db;
        ICollectionView view;
        public PageReports()
        {
            InitializeComponent();
        }

        private void dataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            db = new ServiceDBEntities();
            loadDataGrid();
        }
        public void loadDataGrid()
        {

            var list = db.Transports.ToList();
            view = CollectionViewSource.GetDefaultView(list);
            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = view;
            view.Filter = null;
            setFilters();
        }

        private bool checkCheckBoxes(ref Transports i)
        {
            List<System.Windows.Controls.CheckBox> list = new List<System.Windows.Controls.CheckBox>();
            list.Add(activeCheckBox);
            list.Add(finishedCheckBox);
            list.Add(canceledCheckBox);
            bool ret = false;
            foreach(var elem in list)
            {
                if(elem.IsChecked==true)
                {
                    if(i.Status.name==elem.Content.ToString())
                    {
                        ret = true;
                    }
                }
            }
            list.Clear();
            if (!(bool)activeCheckBox.IsChecked && !(bool)finishedCheckBox.IsChecked && !(bool)canceledCheckBox.IsChecked)
                return true;
            return ret;
        }
        private void setFilters()
        {
            view.Filter = item =>
            {
                var i = item as Transports;
                if (i != null)
                {
                    bool ret = true;
                    if (!string.IsNullOrWhiteSpace(employeeTextBox.Text))
                    {
                        if (!string.IsNullOrWhiteSpace(i.Users.login) && !i.Users.login.Contains(employeeTextBox.Text))
                            ret = false;
                    }
                    ret = checkCheckBoxes(ref i);
                    if (!string.IsNullOrWhiteSpace(fromTextBox.Text))
                    {
                        if (!string.IsNullOrWhiteSpace(i.origin) && !i.origin.Contains(fromTextBox.Text))
                            ret = false;
                    }
                    if (!string.IsNullOrWhiteSpace(toTextBox.Text))
                    {
                        if (!string.IsNullOrWhiteSpace(i.destination) && !i.destination.Contains(fromTextBox.Text))
                            ret = false;
                    }
                    if (dateStartPicker.SelectedDate != null && dateEndPicker.SelectedDate != null)
                    {
                        if (!(i.start_date >= dateStartPicker.SelectedDate.Value && i.start_date < dateEndPicker.SelectedDate.Value))
                            ret = false;
                    }
                    if (weightMinTextBox.Text != "" && weightMaxTextBox.Text != "")
                    {
                        int weightMin = Int32.Parse(weightMinTextBox.Text);
                        int weightMax = Int32.Parse(weightMaxTextBox.Text);
                        if (!(i.weight >= weightMin && i.weight < weightMax))
                            ret = false;
                    }
                    if (!string.IsNullOrWhiteSpace(customerTextBox.Text))
                    {
                        if (!string.IsNullOrWhiteSpace(i.Customers.name) && !i.Customers.name.Contains(customerTextBox.Text))
                            ret = false;
                    }
                    return ret;
                }
                return false;
            };
        }
        private void employeeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            setFilters();
        }
        private void status(object sender, RoutedEventArgs e)
        {
            setFilters();
        }

        private void fromTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            setFilters();
        }

        private void toTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            setFilters();
        }

        private void customerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            setFilters();
        }

        private void dateChanged(object sender, SelectionChangedEventArgs e)
        {
            setFilters();
        }

        private void weightFilter(object sender, TextChangedEventArgs e)
        {
            setFilters();
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            foreach (childItem child in FindVisualChildren<childItem>(obj))
            {
                return child;
            }

            return null;
        }
        private void ExportToPdf(System.Windows.Controls.DataGrid grid,string path)
        {
            PdfPTable table = new PdfPTable(grid.Columns.Count);
            Document doc = new Document(PageSize.LETTER, 10, 10, 42, 35);
            PdfWriter writer = PdfWriter.GetInstance(doc, new System.IO.FileStream(path, System.IO.FileMode.Create));
            doc.Open();
            for (int j = 0; j < grid.Columns.Count; j++)
            {
                table.AddCell(new Phrase(grid.Columns[j].Header.ToString()));
            }
            table.HeaderRows = 1;
            IEnumerable itemsSource = grid.ItemsSource as IEnumerable;
            if (itemsSource != null)
            {
                foreach (var item in itemsSource)
                {
                    DataGridRow row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                    if (row != null)
                    {
                        DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(row);
                        for (int i = 0; i < grid.Columns.Count; ++i)
                        {
                            System.Windows.Controls.DataGridCell cell = (System.Windows.Controls.DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(i);
                            TextBlock txt = cell.Content as TextBlock;
                            if (txt != null)
                            {
                                table.AddCell(new Phrase(txt.Text));
                            }
                        }
                    }
                }

                doc.Add(table);
                doc.Close();
            }
        }
        private void GeneratePDFButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "PDF files (*.pdf)|*.pdf";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ExportToPdf(dataGrid,saveFileDialog1.FileName);
            }
        }
    }
}
