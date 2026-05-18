using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Data;

namespace AutoTrack.Helpers
{
    public class PrintHelper
    {
        private string _printContent;
        private PrintDocument _printDocument;
        private Font _titleFont = new Font("Segoe UI", 20, FontStyle.Bold);
        private Font _headerFont = new Font("Segoe UI", 12, FontStyle.Bold);
        private Font _normalFont = new Font("Segoe UI", 10);
        private Font _smallFont = new Font("Segoe UI", 8);
        private int _yPosition = 100;
        private float _centerX;
        private string _printTitle;
        private DataRow _serviceData;
        private DataRow _customerData;
        private DataRow _vehicleData;
        private DataRow _paymentData;

        public PrintHelper()
        {
            _printDocument = new PrintDocument();
            _printDocument.PrintPage += PrintDocument_PrintPage;
        }

        // Main method for printing service invoice with preview
        public void PrintServiceInvoice(DataRow serviceData, DataRow customerData, DataRow vehicleData, DataRow paymentData)
        {
            _serviceData = serviceData;
            _customerData = customerData;
            _vehicleData = vehicleData;
            _paymentData = paymentData;
            _printTitle = "SERVICE INVOICE";

            // Show Print Preview
            PrintPreviewDialog previewDialog = new PrintPreviewDialog();
            previewDialog.Document = _printDocument;
            previewDialog.WindowState = FormWindowState.Maximized;
            previewDialog.ShowDialog();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            _yPosition = 60;

            // Calculate center X position
            float pageWidth = e.PageBounds.Width;
            _centerX = pageWidth / 2;

            // Safely get values
            string serviceId = _serviceData.Table.Columns.Contains("ServiceID") ? _serviceData["ServiceID"].ToString() : "N/A";
            string jobOrderNo = _serviceData.Table.Columns.Contains("JobOrderNo") ? _serviceData["JobOrderNo"].ToString() : "N/A";
            string serviceType = _serviceData.Table.Columns.Contains("ServiceType") ? _serviceData["ServiceType"].ToString() : "N/A";
            string status = _serviceData.Table.Columns.Contains("Status") ? _serviceData["Status"].ToString() : "N/A";
            string dateIn = _serviceData.Table.Columns.Contains("DateIn") ? Convert.ToDateTime(_serviceData["DateIn"]).ToString("MMMM dd, yyyy") : "N/A";
            string notes = _serviceData.Table.Columns.Contains("Notes") ? _serviceData["Notes"].ToString() : "";

            // Get price values
            decimal laborCost = _serviceData.Table.Columns.Contains("LaborCost") && _serviceData["LaborCost"] != DBNull.Value ? Convert.ToDecimal(_serviceData["LaborCost"]) : 0;
            decimal partsCost = _serviceData.Table.Columns.Contains("PartsCost") && _serviceData["PartsCost"] != DBNull.Value ? Convert.ToDecimal(_serviceData["PartsCost"]) : 0;
            decimal discount = _serviceData.Table.Columns.Contains("Discount") && _serviceData["Discount"] != DBNull.Value ? Convert.ToDecimal(_serviceData["Discount"]) : 0;
            decimal totalCost = _serviceData.Table.Columns.Contains("TotalCost") && _serviceData["TotalCost"] != DBNull.Value ? Convert.ToDecimal(_serviceData["TotalCost"]) : (laborCost + partsCost);
            decimal finalAmount = _serviceData.Table.Columns.Contains("FinalAmount") && _serviceData["FinalAmount"] != DBNull.Value ? Convert.ToDecimal(_serviceData["FinalAmount"]) : (totalCost - discount);

            string customerName = _customerData.Table.Columns.Contains("FirstName") && _customerData.Table.Columns.Contains("LastName")
                ? $"{_customerData["FirstName"]} {_customerData["LastName"]}"
                : (_customerData.Table.Columns.Contains("Customer") ? _customerData["Customer"].ToString() : "N/A");

            string customerPhone = _customerData.Table.Columns.Contains("Phone") ? _customerData["Phone"].ToString() : "N/A";
            string customerEmail = _customerData.Table.Columns.Contains("Email") ? _customerData["Email"].ToString() : "N/A";

            string make = _vehicleData.Table.Columns.Contains("Make") ? _vehicleData["Make"].ToString() : "N/A";
            string model = _vehicleData.Table.Columns.Contains("Model") ? _vehicleData["Model"].ToString() : "N/A";
            string plate = _vehicleData.Table.Columns.Contains("PlateNumber") ? _vehicleData["PlateNumber"].ToString() : "N/A";

            // Draw border
            g.DrawRectangle(Pens.Black, 40, 40, pageWidth - 80, e.MarginBounds.Height - 20);

            // Print Header with border
            string headerText = "AUTO TRACK SERVICE CENTER";
            SizeF headerSize = g.MeasureString(headerText, new Font("Segoe UI", 14, FontStyle.Bold));
            float headerX = _centerX - (headerSize.Width / 2);
            g.DrawString(headerText, new Font("Segoe UI", 14, FontStyle.Bold), Brushes.DarkBlue, headerX, _yPosition);
            _yPosition += 30;

            string addressText = "123 Service Road, Automotive City";
            SizeF addressSize = g.MeasureString(addressText, _smallFont);
            float addressX = _centerX - (addressSize.Width / 2);
            g.DrawString(addressText, _smallFont, Brushes.Gray, addressX, _yPosition);
            _yPosition += 18;

            string contactText = "Tel: (123) 456-7890 | Email: support@autotrack.com";
            SizeF contactSize = g.MeasureString(contactText, _smallFont);
            float contactX = _centerX - (contactSize.Width / 2);
            g.DrawString(contactText, _smallFont, Brushes.Gray, contactX, _yPosition);
            _yPosition += 30;

            // Title
            SizeF titleSize = g.MeasureString(_printTitle, _titleFont);
            float titleX = _centerX - (titleSize.Width / 2);
            g.DrawString(_printTitle, _titleFont, Brushes.Black, titleX, _yPosition);
            _yPosition += 40;

            // Separator line
            g.DrawLine(Pens.Black, 80, _yPosition, pageWidth - 80, _yPosition);
            _yPosition += 20;

            // Invoice Details
            string invoiceText = $"Invoice #: {jobOrderNo}";
            SizeF invoiceSize = g.MeasureString(invoiceText, _normalFont);
            float invoiceX = _centerX - (invoiceSize.Width / 2);
            g.DrawString(invoiceText, _normalFont, Brushes.Black, invoiceX, _yPosition);
            _yPosition += 22;

            string dateText = $"Date: {DateTime.Now:MMMM dd, yyyy HH:mm}";
            SizeF dateSize = g.MeasureString(dateText, _normalFont);
            float dateX = _centerX - (dateSize.Width / 2);
            g.DrawString(dateText, _normalFont, Brushes.Black, dateX, _yPosition);
            _yPosition += 35;

            // ========== CUSTOMER INFORMATION ==========
            g.DrawLine(Pens.LightGray, 80, _yPosition, pageWidth - 80, _yPosition);
            _yPosition += 10;

            string customerHeader = "CUSTOMER INFORMATION";
            SizeF customerHeaderSize = g.MeasureString(customerHeader, _headerFont);
            float customerHeaderX = _centerX - (customerHeaderSize.Width / 2);
            g.DrawString(customerHeader, _headerFont, Brushes.DarkBlue, customerHeaderX, _yPosition);
            _yPosition += 25;

            g.DrawLine(Pens.LightGray, 80, _yPosition, pageWidth - 80, _yPosition);
            _yPosition += 15;

            string customerNameText = $"Name:  {customerName}";
            SizeF customerNameSize = g.MeasureString(customerNameText, _normalFont);
            float customerNameX = _centerX - (customerNameSize.Width / 2);
            g.DrawString(customerNameText, _normalFont, Brushes.Black, customerNameX, _yPosition);
            _yPosition += 22;

            string customerPhoneText = $"Phone: {customerPhone}";
            SizeF customerPhoneSize = g.MeasureString(customerPhoneText, _normalFont);
            float customerPhoneX = _centerX - (customerPhoneSize.Width / 2);
            g.DrawString(customerPhoneText, _normalFont, Brushes.Black, customerPhoneX, _yPosition);
            _yPosition += 22;

            string customerEmailText = $"Email: {customerEmail}";
            SizeF customerEmailSize = g.MeasureString(customerEmailText, _normalFont);
            float customerEmailX = _centerX - (customerEmailSize.Width / 2);
            g.DrawString(customerEmailText, _normalFont, Brushes.Black, customerEmailX, _yPosition);
            _yPosition += 35;

            // ========== VEHICLE INFORMATION ==========
            g.DrawLine(Pens.LightGray, 80, _yPosition, pageWidth - 80, _yPosition);
            _yPosition += 10;

            string vehicleHeader = "VEHICLE INFORMATION";
            SizeF vehicleHeaderSize = g.MeasureString(vehicleHeader, _headerFont);
            float vehicleHeaderX = _centerX - (vehicleHeaderSize.Width / 2);
            g.DrawString(vehicleHeader, _headerFont, Brushes.DarkBlue, vehicleHeaderX, _yPosition);
            _yPosition += 25;

            g.DrawLine(Pens.LightGray, 80, _yPosition, pageWidth - 80, _yPosition);
            _yPosition += 15;

            string vehicleMakeText = $"Make/Model: {make} {model}";
            SizeF vehicleMakeSize = g.MeasureString(vehicleMakeText, _normalFont);
            float vehicleMakeX = _centerX - (vehicleMakeSize.Width / 2);
            g.DrawString(vehicleMakeText, _normalFont, Brushes.Black, vehicleMakeX, _yPosition);
            _yPosition += 22;

            string vehiclePlateText = $"Plate Number: {plate}";
            SizeF vehiclePlateSize = g.MeasureString(vehiclePlateText, _normalFont);
            float vehiclePlateX = _centerX - (vehiclePlateSize.Width / 2);
            g.DrawString(vehiclePlateText, _normalFont, Brushes.Black, vehiclePlateX, _yPosition);
            _yPosition += 35;

            // ========== SERVICE DETAILS ==========
            g.DrawLine(Pens.LightGray, 80, _yPosition, pageWidth - 80, _yPosition);
            _yPosition += 10;

            string serviceHeader = "SERVICE DETAILS";
            SizeF serviceHeaderSize = g.MeasureString(serviceHeader, _headerFont);
            float serviceHeaderX = _centerX - (serviceHeaderSize.Width / 2);
            g.DrawString(serviceHeader, _headerFont, Brushes.DarkBlue, serviceHeaderX, _yPosition);
            _yPosition += 25;

            g.DrawLine(Pens.LightGray, 80, _yPosition, pageWidth - 80, _yPosition);
            _yPosition += 15;

            string serviceTypeText = $"Service Type: {serviceType}";
            SizeF serviceTypeSize = g.MeasureString(serviceTypeText, _normalFont);
            float serviceTypeX = _centerX - (serviceTypeSize.Width / 2);
            g.DrawString(serviceTypeText, _normalFont, Brushes.Black, serviceTypeX, _yPosition);
            _yPosition += 22;

            string serviceDateText = $"Date In: {dateIn}";
            SizeF serviceDateSize = g.MeasureString(serviceDateText, _normalFont);
            float serviceDateX = _centerX - (serviceDateSize.Width / 2);
            g.DrawString(serviceDateText, _normalFont, Brushes.Black, serviceDateX, _yPosition);
            _yPosition += 22;

            string serviceStatusText = $"Status: {status}";
            SizeF serviceStatusSize = g.MeasureString(serviceStatusText, _normalFont);
            float serviceStatusX = _centerX - (serviceStatusSize.Width / 2);
            g.DrawString(serviceStatusText, _normalFont, Brushes.Black, serviceStatusX, _yPosition);
            _yPosition += 22;

            if (!string.IsNullOrEmpty(notes))
            {
                string serviceNotesText = $"Notes: {notes}";
                SizeF serviceNotesSize = g.MeasureString(serviceNotesText, _normalFont);
                float serviceNotesX = _centerX - (serviceNotesSize.Width / 2);
                g.DrawString(serviceNotesText, _normalFont, Brushes.Black, serviceNotesX, _yPosition);
                _yPosition += 22;
            }
            _yPosition += 15;

            // ========== COST BREAKDOWN ==========
            g.DrawLine(Pens.LightGray, 80, _yPosition, pageWidth - 80, _yPosition);
            _yPosition += 10;

            string costHeader = "COST BREAKDOWN";
            SizeF costHeaderSize = g.MeasureString(costHeader, _headerFont);
            float costHeaderX = _centerX - (costHeaderSize.Width / 2);
            g.DrawString(costHeader, _headerFont, Brushes.DarkBlue, costHeaderX, _yPosition);
            _yPosition += 25;

            g.DrawLine(Pens.LightGray, 80, _yPosition, pageWidth - 80, _yPosition);
            _yPosition += 15;

            // Cost items in a table format
            string laborText = $"Labor Cost:                          ₱{laborCost:N2}";
            SizeF laborSize = g.MeasureString(laborText, _normalFont);
            float laborX = _centerX - (laborSize.Width / 2);
            g.DrawString(laborText, _normalFont, Brushes.Black, laborX, _yPosition);
            _yPosition += 22;

            string partsText = $"Parts Cost:                          ₱{partsCost:N2}";
            SizeF partsSize = g.MeasureString(partsText, _normalFont);
            float partsX = _centerX - (partsSize.Width / 2);
            g.DrawString(partsText, _normalFont, Brushes.Black, partsX, _yPosition);
            _yPosition += 22;

            string subtotalText = $"Subtotal:                            ₱{totalCost:N2}";
            SizeF subtotalSize = g.MeasureString(subtotalText, _normalFont);
            float subtotalX = _centerX - (subtotalSize.Width / 2);
            g.DrawString(subtotalText, new Font("Segoe UI", 10, FontStyle.Bold), Brushes.Black, subtotalX, _yPosition);
            _yPosition += 22;

            if (discount > 0)
            {
                string discountText = $"Discount:                           -₱{discount:N2}";
                SizeF discountSize = g.MeasureString(discountText, _normalFont);
                float discountX = _centerX - (discountSize.Width / 2);
                g.DrawString(discountText, _normalFont, Brushes.Red, discountX, _yPosition);
                _yPosition += 22;
            }

            g.DrawLine(Pens.Black, 80, _yPosition, pageWidth - 80, _yPosition);
            _yPosition += 10;

            string totalText = $"TOTAL AMOUNT:                       ₱{finalAmount:N2}";
            SizeF totalSize = g.MeasureString(totalText, new Font("Segoe UI", 14, FontStyle.Bold));
            float totalX = _centerX - (totalSize.Width / 2);
            g.DrawString(totalText, new Font("Segoe UI", 14, FontStyle.Bold), Brushes.DarkGreen, totalX, _yPosition);
            _yPosition += 45;

            // ========== THANK YOU ==========
            g.DrawLine(Pens.LightGray, 80, _yPosition, pageWidth - 80, _yPosition);
            _yPosition += 15;

            string thankYouText = "THANK YOU FOR YOUR BUSINESS!";
            SizeF thankYouSize = g.MeasureString(thankYouText, _headerFont);
            float thankYouX = _centerX - (thankYouSize.Width / 2);
            g.DrawString(thankYouText, _headerFont, Brushes.DarkBlue, thankYouX, _yPosition);
            _yPosition += 28;

            string thankYouMsg = "We appreciate your trust in AutoTrack Service Center";
            SizeF thankYouMsgSize = g.MeasureString(thankYouMsg, _normalFont);
            float thankYouMsgX = _centerX - (thankYouMsgSize.Width / 2);
            g.DrawString(thankYouMsg, _normalFont, Brushes.Gray, thankYouMsgX, _yPosition);
            _yPosition += 30;

            // ========== FOOTER ==========
            string footerText = $"Printed on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            SizeF footerSize = g.MeasureString(footerText, _smallFont);
            float footerX = _centerX - (footerSize.Width / 2);
            g.DrawString(footerText, _smallFont, Brushes.Gray, footerX, e.PageBounds.Height - 60);

            e.HasMorePages = false;
        }
    }
}