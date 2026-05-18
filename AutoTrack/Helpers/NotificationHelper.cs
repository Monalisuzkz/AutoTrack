using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using AutoTrack.Database;

namespace AutoTrack.Helpers
{
    public class NotificationHelper
    {
        // =============================================
        // CORE NOTIFICATION METHODS
        // =============================================

        public static void SendInAppNotification(int userId, string title, string message, string notificationType = "Info", int relatedId = 0)
        {
            try
            {
                string query = @"
                    INSERT INTO Notifications (UserID, Title, Message, NotificationType, RelatedID, CreatedAt)
                    VALUES (@UserID, @Title, @Message, @Type, @RelatedID, GETDATE())";

                SqlParameter[] parameters = {
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Message", message),
                    new SqlParameter("@Type", notificationType),
                    new SqlParameter("@RelatedID", relatedId)
                };

                DatabaseHelper.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Notification error: " + ex.Message);
            }
        }

        // Send to specific role
        public static void SendToRole(string role, string title, string message, string notificationType = "Info", int relatedId = 0)
        {
            try
            {
                DataTable users = DatabaseHelper.ExecuteQuery(
                    "SELECT UserID FROM Users WHERE Role = @Role AND IsActive = 1",
                    new[] { new SqlParameter("@Role", role) });

                foreach (DataRow user in users.Rows)
                {
                    SendInAppNotification(Convert.ToInt32(user["UserID"]), title, message, notificationType, relatedId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending to {role}: {ex.Message}");
            }
        }

        // Send to multiple roles
        public static void SendToRoles(string[] roles, string title, string message, string notificationType = "Info", int relatedId = 0)
        {
            foreach (string role in roles)
            {
                SendToRole(role, title, message, notificationType, relatedId);
            }
        }

        // Send to specific technician (for job assignments)
        public static void SendToTechnician(int technicianUserId, string title, string message, int serviceId = 0)
        {
            SendInAppNotification(technicianUserId, title, message, "Reminder", serviceId);
        }

        // Old method - keep for compatibility
        public static void SendNotificationToAdmins(string title, string message, string notificationType = "Info")
        {
            string[] roles = { "Admin", "SuperAdmin" };
            SendToRoles(roles, title, message, notificationType);
        }

        public static DataTable GetUnreadNotifications(int userId)
        {
            return DatabaseHelper.ExecuteQuery(
                "SELECT TOP 20 NotificationID, Title, Message, NotificationType, CreatedAt, RelatedID " +
                "FROM Notifications WHERE UserID = @UserID AND IsRead = 0 " +
                "ORDER BY CreatedAt DESC",
                new[] { new SqlParameter("@UserID", userId) });
        }

        public static DataTable GetAllNotifications(int userId)
        {
            return DatabaseHelper.ExecuteQuery(
                "SELECT TOP 50 NotificationID, Title, Message, NotificationType, IsRead, CreatedAt, RelatedID " +
                "FROM Notifications WHERE UserID = @UserID " +
                "ORDER BY CreatedAt DESC",
                new[] { new SqlParameter("@UserID", userId) });
        }

        public static void MarkAsRead(int notificationId)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Notifications SET IsRead = 1 WHERE NotificationID = @ID",
                new[] { new SqlParameter("@ID", notificationId) });
        }

        public static void MarkAllAsRead(int userId)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Notifications SET IsRead = 1 WHERE UserID = @UserID AND IsRead = 0",
                new[] { new SqlParameter("@UserID", userId) });
        }

        public static int GetUnreadCount(int userId)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Notifications WHERE UserID = @UserID AND IsRead = 0",
                new[] { new SqlParameter("@UserID", userId) });
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // =============================================
        // SERVICE NOTIFICATIONS
        // =============================================

        // When new service is assigned to technician
        public static void SendNewServiceAssignment(int serviceId, int technicianUserId, string serviceType, string plateNumber)
        {
            SendToTechnician(technicianUserId, "New Service Assigned",
                $"You have been assigned to {serviceType} for vehicle {plateNumber}.", serviceId);
        }

        // Daily service reminder for tomorrow's services
        public static void SendServiceReminderToStaff(int serviceId, string serviceType, string plateNumber, DateTime dateIn)
        {
            SendToRole("Staff", "Service Tomorrow",
                $"Service: {serviceType} for {plateNumber} is scheduled for {dateIn:MMMM dd, yyyy} at {dateIn:hh:mm tt}.",
                "Reminder", serviceId);
        }

        // =============================================
        // INVENTORY NOTIFICATIONS
        // =============================================

        // Low stock alert - Admin and Supplier
        public static void SendLowStockAlert(int partId, string partName, int quantity, int reorderLevel, int supplierUserId = 0)
        {
            // Send to Admin
            SendToRole("Admin", "Low Stock Alert",
                $"{partName}: Only {quantity} units left. Reorder level is {reorderLevel}.",
                "Warning", partId);

            // Send to Supplier if they have a user account
            if (supplierUserId > 0)
            {
                SendToTechnician(supplierUserId, "Restock Needed",
                    $"{partName} needs restock. Only {quantity} units left.", partId);
            }
        }

        // Out of stock alert - Admin and Supplier
        public static void SendOutOfStockAlert(int partId, string partName, int supplierUserId = 0)
        {
            // Send to Admin
            SendToRole("Admin", "Out of Stock Alert",
                $"{partName} is out of stock! Please reorder immediately.",
                "Alert", partId);

            // Send to Supplier
            if (supplierUserId > 0)
            {
                SendToTechnician(supplierUserId, "URGENT: Out of Stock",
                    $"{partName} is completely out of stock! Immediate restock needed.", partId);
            }
        }

        // =============================================
        // PAYMENT NOTIFICATIONS
        // =============================================

        public static void SendPaymentNotification(int serviceId, decimal amount, string customerName, string paymentMethod)
        {
            string title = "Payment Received";
            string message = $"₱{amount:N2} payment received from {customerName} via {paymentMethod}.";

            // Send to Admin and SuperAdmin
            string[] roles = { "Admin", "SuperAdmin" };
            SendToRoles(roles, title, message, "Success", serviceId);
        }

        // =============================================
        // SUBSCRIPTION NOTIFICATIONS
        // =============================================

        public static void SendSubscriptionExpiringNotification(int subscriptionId, string customerName, string planName, decimal monthlyFee, int daysLeft)
        {
            string title = "Subscription Expiring";
            string message = $"{customerName}'s {planName} subscription expires in {daysLeft} days. Monthly fee: ₱{monthlyFee:N2}";

            // Send to Admin and SuperAdmin
            string[] roles = { "Admin", "SuperAdmin" };
            SendToRoles(roles, title, message, "Warning", subscriptionId);
        }

        // =============================================
        // SCHEDULED CHECKS (Call on Timer)
        // =============================================

        public static void RunScheduledChecks()
        {
            CheckAndSendServiceReminders();
            CheckAndSendSubscriptionReminders();
            CheckAndSendInventoryAlerts();
        }

        private static void CheckAndSendServiceReminders()
        {
            try
            {
                // Get services scheduled for tomorrow with technician info
                DataTable upcomingServices = DatabaseHelper.ExecuteQuery(@"
                    SELECT s.ServiceID, s.DateIn, s.ServiceType, s.JobOrderNo,
                           v.PlateNumber,
                           t.TechnicianID, techUser.UserID as TechnicianUserID
                    FROM ServiceRecords s
                    JOIN Vehicles v ON s.VehicleID = v.VehicleID
                    LEFT JOIN Technicians t ON s.TechnicianID = t.TechnicianID
                    LEFT JOIN Users techUser ON t.UserID = techUser.UserID
                    WHERE s.DateIn = CAST(DATEADD(DAY, 1, GETDATE()) AS DATE)
                    AND s.Status IN ('Pending', 'InProgress')");

                foreach (DataRow service in upcomingServices.Rows)
                {
                    int serviceId = Convert.ToInt32(service["ServiceID"]);
                    string serviceType = service["ServiceType"].ToString();
                    string plateNumber = service["PlateNumber"].ToString();
                    DateTime dateIn = Convert.ToDateTime(service["DateIn"]);

                    // Send to Staff
                    SendServiceReminderToStaff(serviceId, serviceType, plateNumber, dateIn);

                    // Send to assigned Technician
                    if (service["TechnicianUserID"] != DBNull.Value)
                    {
                        SendToTechnician(Convert.ToInt32(service["TechnicianUserID"]), "Your Service Tomorrow",
                            $"You are assigned to {serviceType} for {plateNumber} tomorrow at {dateIn:hh:mm tt}.",
                            serviceId);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Service reminder error: " + ex.Message);
            }
        }

        private static void CheckAndSendSubscriptionReminders()
        {
            try
            {
                DataTable expiringSubscriptions = DatabaseHelper.ExecuteQuery(@"
                    SELECT cs.SubscriptionID, cs.EndDate, sp.PlanName, sp.MonthlyFee,
                           c.FirstName, c.LastName
                    FROM CustomerSubscriptions cs
                    JOIN Customers c ON cs.CustomerID = c.CustomerID
                    JOIN SubscriptionPlans sp ON cs.PlanID = sp.PlanID
                    WHERE cs.Status = 'Active'
                    AND DATEDIFF(DAY, GETDATE(), cs.EndDate) BETWEEN 1 AND 7");

                foreach (DataRow sub in expiringSubscriptions.Rows)
                {
                    int daysLeft = (Convert.ToDateTime(sub["EndDate"]) - DateTime.Now).Days;
                    string customerName = $"{sub["FirstName"]} {sub["LastName"]}";
                    string planName = sub["PlanName"].ToString();
                    decimal monthlyFee = Convert.ToDecimal(sub["MonthlyFee"]);
                    int subscriptionId = Convert.ToInt32(sub["SubscriptionID"]);

                    SendSubscriptionExpiringNotification(subscriptionId, customerName, planName, monthlyFee, daysLeft);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Subscription reminder error: " + ex.Message);
            }
        }

        private static void CheckAndSendInventoryAlerts()
        {
            try
            {
                // Get low stock items with supplier info
                DataTable lowStock = DatabaseHelper.ExecuteQuery(@"
                    SELECT i.PartID, i.PartName, i.Quantity, i.ReorderLevel,
                           s.SupplierID, u.UserID as SupplierUserID
                    FROM Inventory i
                    LEFT JOIN Suppliers s ON i.SupplierID = s.SupplierID
                    LEFT JOIN Users u ON s.ContactPerson = u.FullName
                    WHERE i.Quantity <= i.ReorderLevel AND i.Quantity > 0");

                foreach (DataRow item in lowStock.Rows)
                {
                    int partId = Convert.ToInt32(item["PartID"]);
                    string partName = item["PartName"].ToString();
                    int quantity = Convert.ToInt32(item["Quantity"]);
                    int reorderLevel = Convert.ToInt32(item["ReorderLevel"]);
                    int supplierUserId = item["SupplierUserID"] != DBNull.Value ? Convert.ToInt32(item["SupplierUserID"]) : 0;

                    // Check if alert was already sent today
                    DataTable existing = DatabaseHelper.ExecuteQuery(@"
                        SELECT NotificationID FROM Notifications 
                        WHERE RelatedID = @PartID 
                        AND Title = 'Low Stock Alert'
                        AND CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE)",
                        new[] { new SqlParameter("@PartID", partId) });

                    if (existing.Rows.Count == 0)
                    {
                        SendLowStockAlert(partId, partName, quantity, reorderLevel, supplierUserId);
                    }
                }

                // Check for out of stock with supplier info
                DataTable outOfStock = DatabaseHelper.ExecuteQuery(@"
                    SELECT i.PartID, i.PartName, s.SupplierID, u.UserID as SupplierUserID
                    FROM Inventory i
                    LEFT JOIN Suppliers s ON i.SupplierID = s.SupplierID
                    LEFT JOIN Users u ON s.ContactPerson = u.FullName
                    WHERE i.Quantity = 0");

                foreach (DataRow item in outOfStock.Rows)
                {
                    int partId = Convert.ToInt32(item["PartID"]);
                    string partName = item["PartName"].ToString();
                    int supplierUserId = item["SupplierUserID"] != DBNull.Value ? Convert.ToInt32(item["SupplierUserID"]) : 0;

                    DataTable existing = DatabaseHelper.ExecuteQuery(@"
                        SELECT NotificationID FROM Notifications 
                        WHERE RelatedID = @PartID 
                        AND Title = 'Out of Stock Alert'
                        AND CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE)",
                        new[] { new SqlParameter("@PartID", partId) });

                    if (existing.Rows.Count == 0)
                    {
                        SendOutOfStockAlert(partId, partName, supplierUserId);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Inventory alert error: " + ex.Message);
            }
        }
    }
}