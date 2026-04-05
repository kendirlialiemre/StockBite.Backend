namespace StockBite.Domain.Constants;

public static class Permissions
{
    public static class Menu
    {
        public const string View = "Menu.View";
        public const string Edit = "Menu.Edit";
    }

    public static class Orders
    {
        public const string View = "Orders.View";
        public const string Create = "Orders.Create";
        public const string Close = "Orders.Close";
        public const string Cancel = "Orders.Cancel";
    }

    public static class Stock
    {
        public const string View = "Stock.View";
        public const string AddItem = "Stock.AddItem";
        public const string EditItem = "Stock.EditItem";
        public const string AddMovement = "Stock.AddMovement";
    }

    public static class ProfitLoss
    {
        public const string View = "ProfitLoss.View";
    }

    public static class Tables
    {
        public const string View = "Tables.View";
        public const string Manage = "Tables.Manage";
    }

    public static IEnumerable<string> All() =>
    [
        Menu.View, Menu.Edit,
        Orders.View, Orders.Create, Orders.Close, Orders.Cancel,
        Stock.View, Stock.AddItem, Stock.EditItem, Stock.AddMovement,
        ProfitLoss.View,
        Tables.View, Tables.Manage
    ];
}
