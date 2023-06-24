using System.Text.Json;
// Methods for managing servicing items.
namespace BikeServicing.Data
{
    public class ItemServices
    {
        // To create a list collection nd save all servicing items in a file.
        public static void SaveItems(List<ServicingItems> items)
        {
            string directoryPath = Utils.GetAppDirectoryPath();
            string itemFilePath = Utils.GetItemsFilePath();

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var serializedUserJson = JsonSerializer.Serialize(items);
            File.WriteAllText(itemFilePath, serializedUserJson);
        }

        // To create a list collection of all taken out servicing items.
        public static void SaveItemsLogs(List<InventoryRecord> itemHistory)
        {
            string directoryPath = Utils.GetAppDirectoryPath();
            string itemHistoryFilePath = Utils.GetItemsRecordFilePath();

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var serializedUserJson = JsonSerializer.Serialize(itemHistory);
            File.WriteAllText(itemHistoryFilePath, serializedUserJson);
        }

        // To create a list collection to fetch all servicing items.
        public static List<ServicingItems> FetchItem()
        {
            string itemFilePath = Utils.GetItemsFilePath();

            if (!File.Exists(itemFilePath))
            {
                return new List<ServicingItems>();
            }

            var items = File.ReadAllText(itemFilePath);

            return JsonSerializer.Deserialize<List<ServicingItems>>(items);
        }

        // To fetch all servicing items by id.
        public static ServicingItems FetchItemById(Guid Id)
        {
            List<ServicingItems> items = FetchItem();
            ServicingItems item = items.FirstOrDefault(item => item.Id == Id);

            if (item == null)
            {
                throw new Exception("Item is missing.");
            }

            return item;
        }

        // To create a list collection to fetch all item records.
        public static List<InventoryRecord> FetchItemsRecord()
        {
            string itemHistoryFilePath = Utils.GetItemsRecordFilePath();

            if (!File.Exists(itemHistoryFilePath))
            {
                return new List<InventoryRecord>();
            }

            var itemHistory = File.ReadAllText(itemHistoryFilePath);

            return JsonSerializer.Deserialize<List<InventoryRecord>>(itemHistory);
        }

        // To create a list collection of all rejected requests.
        public static List<InventoryRecord> FetchRejectedItemHistory()
        {
            List<InventoryRecord> records = FetchItemsRecord();

            List<InventoryRecord> rejectedRecord = records.FindAll(record => record.Action == RequestStatus.Reject);

            return rejectedRecord;
        }

        // To create a list collection of all approved requests.
        public static List<InventoryRecord> FetchApprovedItemHistory()
        {
            List<InventoryRecord> records = FetchItemsRecord();

            List<InventoryRecord> approvedRecord = records.FindAll(record => record.Action == RequestStatus.Approve);

            return approvedRecord;
        }

        // To create a list collection of all pending requests.
        public static List<InventoryRecord> FetchPendingItemHistory()
        {
            List<InventoryRecord> records = FetchItemsRecord();

            List<InventoryRecord> pendingRecord = records.FindAll(record => record.Action == RequestStatus.Pending);

            return pendingRecord;
        }

        // To create a list collection and add new items to the list.
        public static List<ServicingItems> AddItem(Guid addedBy, string itemName, int quantity, float unitPrice)
        {
            List<ServicingItems> items = FetchItem();

            items.Add(
                new ServicingItems
                {
                    AddedBy = addedBy,
                    Name = itemName,
                    Quantity = quantity,
                    UnitPrice = unitPrice
                }
            );

            SaveItems(items);
            return items;
        }

        // To create a list collection of item added to records.
        public static List<InventoryRecord> AddItemHistory(Guid itemId, Guid takerId, int quantity)
        {
            List<InventoryRecord> itemHistory = FetchItemsRecord();

            itemHistory.Add(
                new InventoryRecord
                {
                    ItemId = itemId,
                    TakenBy = takerId,
                    Quantity = quantity
                }
            );

            SaveItemsLogs(itemHistory);
            return itemHistory;
        }

        // To delete a certain thing from collection.
        public static List<ServicingItems> DeleteItem(Guid Id)
        {
            List<ServicingItems> items = FetchItem();
            ServicingItems item = items.FirstOrDefault(item => item.Id == Id);

            if (item == null)
            {
                throw new Exception("Item not found.");
            }

            List<InventoryRecord> Records = FetchItemsRecord();

            Records.RemoveAll(record => item.Id == record.ItemId);
            SaveItemsLogs(Records);

            items.Remove(item);
            SaveItems(items);
            return items;
        }

        // To update a items detail in the list collection.
        public static List<ServicingItems> UpdateItem(Guid Id, string itemName, int quantity, float unitPrice)
        {
            List<ServicingItems> items = FetchItem();
            ServicingItems item = items.FirstOrDefault(item => item.Id == Id);

            if (item == null)
            {
                throw new Exception("Item not found.");
            }

            item.UnitPrice = unitPrice;
            item.Quantity = quantity;
            item.Name = itemName;

            SaveItems(items);
            return items;
        }

        // To create a list collection of items taken out, and the validation for it.
        public static List<ServicingItems> TakeOutItem(Guid itemId, Guid takerId, int quantity)
        {
            if (quantity < 1)
            {
                throw new Exception("Takeout quantity must be atleast 1!");
            }

            List<ServicingItems> items = FetchItem();
            ServicingItems item = items.FirstOrDefault(item => item.Id == itemId);

            if (item == null)
            {
                throw new Exception("Item not found.");
            }

            if (item.Quantity < quantity)
            {
                throw new Exception("Quantity not available!");
            }

            User user = UsersServices.GetById(takerId);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            item.Quantity -= quantity;
            SaveItems(items);
            AddItemHistory(itemId, takerId, quantity);
            return items;
        }

        // To create a list collection for taken out items and validation for that certain item.
        public static List<InventoryRecord> HandleTakeAction(Guid RecordId, Guid actionId, RequestStatus action)
        {
            List<InventoryRecord> Records = FetchItemsRecord();
            InventoryRecord record = Records.FirstOrDefault(record => record.Id == RecordId);


            if (record == null)
            {
                throw new Exception("Records not found.");
            }

            List<ServicingItems> items = FetchItem();
            ServicingItems item = items.FirstOrDefault(item => item.Id == record.ItemId);

            if (action == RequestStatus.Reject)
            {
                record.Action = RequestStatus.Reject;
                item.Quantity += record.Quantity;
            }

            if (action == RequestStatus.Approve)
            {
                string message = "Only able to approve from monday-friday and 9am to 4pm";
                if (DateTime.Parse("12: 00 AM") >= DateTime.Now || DateTime.Parse("11: 00 PM") <= DateTime.Now)
                {
                    throw new Exception(message);
                }

                if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                {
                    throw new Exception(message);
                }

                record.Action = RequestStatus.Approve;
            }

            record.ApprovedBy = actionId;
            record.DateApproved = DateTime.Now;

            SaveItems(items);
            SaveItemsLogs(Records);
            return Records;
        }

        // To create a list collection of items taken out along with the approved date.
        public static List<ItemsChart> ItemTakenData()
        {
            List<InventoryRecord> records = FetchApprovedItemHistory();
            List<ItemsChart> datas = new();
            foreach (InventoryRecord record in records)
            {
                var exist = datas.FirstOrDefault(data => data.ItemId == record.ItemId);
                if (exist != null)
                {
                    exist.Quantity += record.Quantity;
                    continue;
                }

                datas.Add(
                    new ItemsChart
                    {
                        ItemId = record.ItemId,
                        Quantity = record.Quantity,
                    }
                    );
            }

            return datas;
        }
    }
}
