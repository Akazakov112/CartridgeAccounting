using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CartAccLibrary.Comparers;
using CartAccLibrary.Services;

namespace CartAccLibrary.Dto
{
    public class OspDataDTO : BaseVm
    {
        private UserDTO currentUser;
        private ObservableCollection<AccessDTO> accesses;
        private ObservableCollection<PrinterDTO> printers;
        private ObservableCollection<CartridgeDTO> cartridges;
        private ObservableCollection<OspDTO> osps;
        private ObservableCollection<UserDTO> users;
        private ObservableCollection<BalanceDTO> balance;
        private ObservableCollection<ExpenseDTO> userExpenses, expenses;
        private ObservableCollection<ReceiptDTO> receipts;
        private ObservableCollection<ProviderDTO> providers;
        private ObservableCollection<EmailDTO> emails;


        /// <summary>
        /// Событие обновления всех данных.
        /// </summary>
        public event Action AllDataUpdated;

        /// <summary>
        /// Событие обновления баланса.
        /// </summary>
        public event Action BalanceUpdated;


        /// <summary>
        /// Текущий пользователь.
        /// </summary>
        public UserDTO CurrentUser 
        {
            get { return currentUser; }
            set { currentUser = value; RaisePropertyChanged(nameof(CurrentUser)); }
        }

        /// <summary>
        /// Все уровни доступа.
        /// </summary>
        public ObservableCollection<AccessDTO> Accesses
        {
            get { return accesses; }
            set { accesses = value; RaisePropertyChanged(nameof(Accesses)); }
        }

        /// <summary>
        /// Все принтеры.
        /// </summary>
        public ObservableCollection<PrinterDTO> Printers
        {
            get { return printers; }
            set { printers = value; RaisePropertyChanged(nameof(Printers)); }
        }

        /// <summary>
        /// Все картриджи.
        /// </summary>
        public ObservableCollection<CartridgeDTO> Cartridges
        {
            get { return cartridges; }
            set { cartridges = value; RaisePropertyChanged(nameof(Cartridges)); }
        }

        /// <summary>
        /// Все ОСП.
        /// </summary>
        public ObservableCollection<OspDTO> Osps
        {
            get { return osps; }
            set { osps = value; RaisePropertyChanged(nameof(Osps)); }
        }

        /// <summary>
        /// Все пользователи.
        /// </summary>
        public ObservableCollection<UserDTO> Users
        {
            get { return users; }
            set { users = value; RaisePropertyChanged(nameof(Users)); }
        }

        /// <summary>
        /// Баланс ОСП.
        /// </summary>
        public ObservableCollection<BalanceDTO> Balance
        {
            get { return balance; }
            set { balance = value; RaisePropertyChanged(nameof(Balance)); }
        }

        /// <summary>
        /// Списания пользователя.
        /// </summary>
        public ObservableCollection<ExpenseDTO> UserExpenses
        {
            get { return userExpenses; }
            set { userExpenses = value; RaisePropertyChanged(nameof(UserExpenses)); }
        }

        /// <summary>
        /// Списания ОСП.
        /// </summary>
        public ObservableCollection<ExpenseDTO> Expenses
        {
            get { return expenses; }
            set { expenses = value; RaisePropertyChanged(nameof(Expenses)); }
        }

        /// <summary>
        /// Поступления ОСП.
        /// </summary>
        public ObservableCollection<ReceiptDTO> Receipts
        {
            get { return receipts; }
            set { receipts = value; RaisePropertyChanged(nameof(Receipts)); }
        }

        /// <summary>
        /// Поставщики.
        /// </summary>
        public ObservableCollection<ProviderDTO> Providers
        {
            get { return providers; }
            set { providers = value; RaisePropertyChanged(nameof(Providers)); }
        }

        /// <summary>
        /// Адреса электронной почты ОСП.
        /// </summary>
        public ObservableCollection<EmailDTO> Emails
        {
            get { return emails; }
            set { emails = value; RaisePropertyChanged(nameof(Emails)); }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public OspDataDTO() 
        {
            CurrentUser = new UserDTO();
            Accesses = new ObservableCollection<AccessDTO>();
            Printers = new ObservableCollection<PrinterDTO>();
            Cartridges = new ObservableCollection<CartridgeDTO>();
            Osps = new ObservableCollection<OspDTO>();
            Users = new ObservableCollection<UserDTO>();
            Balance = new ObservableCollection<BalanceDTO>();
            UserExpenses = new ObservableCollection<ExpenseDTO>();
            Expenses = new ObservableCollection<ExpenseDTO>();
            Receipts = new ObservableCollection<ReceiptDTO>();
            Providers = new ObservableCollection<ProviderDTO>();
            Emails = new ObservableCollection<EmailDTO>();
        }


        /// <summary>
        /// Обновляет все данные.
        /// </summary>
        /// <param name="newOspData">Новые данные ОСП</param>
        public void UpdateAll(OspDataDTO newOspData)
        {
            if (newOspData != null)
            {
                foreach (var prop in newOspData.GetType().GetProperties())
                {
                    prop.SetValue(this, prop.GetValue(newOspData));
                }
                AllDataUpdated?.Invoke();
            }
        }

        /// <summary>
        /// Обновляет все картриджи баланса ОСП.
        /// </summary>
        /// <param name="ospBalance">Новый список баланса</param>
        public void UpdateBalances(IEnumerable<BalanceDTO> ospBalance)
        {
            // Если список не пустой обновить состояние каждого объекта.
            if (ospBalance.Any())
            {
                foreach (var balance in ospBalance)
                {
                    UpdateBalance(balance);
                }
                BalanceUpdated?.Invoke();
            }
            // Иначе очистить текущий список.
            else
            {
                Balance.Clear();
            }
        }

        /// <summary>
        /// Обновляет списания ОСП.
        /// </summary>
        /// <param name="ospExpenses">Новый список списаний</param>
        public void UpdateExpenses(IEnumerable<ExpenseDTO> ospExpenses)
        {
            // Если список не пустой.
            if (ospExpenses.Count() > 0)
            {
                // Получить новые объекты.
                ExpenseDTO[] added = ospExpenses.Except(Expenses, new ExpenseDTOComparer()).ToArray();
                // Получить удаленные объекты
                ExpenseDTO[] removed = Expenses.Except(ospExpenses, new ExpenseDTOComparer()).ToArray();
                // Получить существующие объекты.
                ExpenseDTO[] existing = Expenses.Intersect(ospExpenses, new ExpenseDTOComparer()).ToArray();
                // Добавить в список новые объекты.
                if (added.Any())
                {
                    for (int i = 0; i < added.Count(); i++)
                    {
                        Expenses.Add(added[i]);
                    }
                }
                // Убрать из списка удаленные объекты.
                if (removed.Any())
                {
                    for (int i = 0; i < removed.Count(); i++)
                    {
                        Expenses.Remove(removed[i]);
                    }
                }
                // Обновить свойства существующим объектам.
                if (existing.Any())
                {
                    foreach (var exist in existing)
                    {
                        UpdateExpense(exist);
                    }
                }
            }
            // Если список новых пустой очистить список списаний.
            else
            {
                Expenses.Clear();
            }

        }

        /// <summary>
        /// Обновляет списания пользователя.
        /// </summary>
        /// <param name="ospUserExpenses">Новый список списаний пользователя</param>
        public void UpdateUserExpenses(IEnumerable<ExpenseDTO> ospUserExpenses)
        {
            // Если список не пустой.
            if (ospUserExpenses.Count() > 0)
            {
                // Получить новые объекты.
                ExpenseDTO[] added = ospUserExpenses.Except(UserExpenses, new ExpenseDTOComparer()).ToArray();
                // Получить удаленные объекты
                ExpenseDTO[] removed = UserExpenses.Except(ospUserExpenses, new ExpenseDTOComparer()).ToArray();
                // Получить существующие объекты.
                ExpenseDTO[] existing = UserExpenses.Intersect(ospUserExpenses, new ExpenseDTOComparer()).ToArray();
                // Добавить в список новые объекты.
                if (added.Any())
                {
                    for (int i = 0; i < added.Count(); i++)
                    {
                        UserExpenses.Add(added[i]);
                    }
                }
                // Убрать из списка удаленные объекты.
                if (removed.Any())
                {
                    for (int i = 0; i < removed.Count(); i++)
                    {
                        UserExpenses.Remove(removed[i]);
                    }
                }
                // Обновить свойства существующим объектам.
                if (existing.Any())
                {
                    foreach (var exist in existing)
                    {
                        UpdateUserExpense(exist);
                    }
                }
            }
            // Если список новых пустой очистить список списаний пользователя.
            else
            {
                UserExpenses.Clear();
            }
        }

        /// <summary>
        /// Обновляет поступления ОСП.
        /// </summary>
        /// <param name="ospReceipts">Новый список поступлений</param>
        public void UpdateReceipts(IEnumerable<ReceiptDTO> ospReceipts)
        {
            // Если список не пустой.
            if (ospReceipts.Count() > 0)
            {
                // Получить новые объекты.
                ReceiptDTO[] added = ospReceipts.Except(Receipts, new ReceiptDTOComparer()).ToArray();
                // Получить удаленные объекты
                ReceiptDTO[] removed = Receipts.Except(ospReceipts, new ReceiptDTOComparer()).ToArray();
                // Получить существующие объекты.
                ReceiptDTO[] existing = Receipts.Intersect(ospReceipts, new ReceiptDTOComparer()).ToArray();
                // Добавить в список новые объекты.
                if (added.Any())
                {
                    for (int i = 0; i < added.Count(); i++)
                    {
                        Receipts.Add(added[i]);
                    }
                }
                // Убрать из списка удаленные объекты.
                if (removed.Any())
                {
                    for (int i = 0; i < removed.Count(); i++)
                    {
                        Receipts.Remove(removed[i]);
                    }
                }
                // Обновить свойства существующим объектам.
                if (existing.Any())
                {
                    foreach (var exist in existing)
                    {
                        UpdateReceipt(exist);
                    }
                }
            }
            // Если список новых пустой очистить список поступлений.
            else
            {
                Receipts.Clear();
            }
        }

        /// <summary>
        /// Обновляет все картриджи в словаре.
        /// </summary>
        /// <param name="allCartridges">Новый список картриджей</param>
        public void UpdateCartridges(IEnumerable<CartridgeDTO> allCartridges)
        {
            // Если список не пустой обновить состояние каждого объекта.
            if (allCartridges.Any())
            {
                foreach (var cartridge in allCartridges)
                {
                    UpdateCartridge(cartridge);
                }
            }
            // Иначе очистить текущий список.
            else
            {
                Cartridges.Clear();
            }
        }

        /// <summary>
        /// Обновляет все принтеры в словаре.
        /// </summary>
        /// <param name="allPrinters">Новый список принтеров</param>
        public void UpdatePrinters(IEnumerable<PrinterDTO> allPrinters)
        {
            // Если список не пустой обновить состояние каждого объекта.
            if (allPrinters.Any())
            {
                foreach (var printer in allPrinters)
                {
                    UpdatePrinter(printer);
                }
            }
            // Иначе очистить текущий список.
            else
            {
                Printers.Clear();
            }
        }


        /// <summary>
        /// Обновляет один картридж баланса.
        /// </summary>
        /// <param name="editedBalance">Отредактированный картридж баланса</param>
        public void UpdateBalance(BalanceDTO editedBalance)
        {
            // Найти баланс, равное редактированному, в текущем списке баланса ОСП.
            BalanceDTO balance = Balance.FirstOrDefault(x => x.Id == editedBalance.Id);
            // Если баланс найден.
            if (balance != null)
            {
                // Присвоить значения свойств отредактированного баланса текущему.
                foreach (var prop in balance.GetType().GetProperties())
                {
                    prop.SetValue(balance, prop.GetValue(editedBalance));
                }
            }
            else
            {
                Balance.Add(editedBalance);
            }
        }

        /// <summary>
        /// Обновляет одно списание.
        /// </summary>
        /// <param name="editedExpense">Отредактированное списание</param>
        public void UpdateExpense(ExpenseDTO editedExpense)
        {
            // Найти списание, равное редактированному, в текущем списке списаний ОСП.
            ExpenseDTO expense = Expenses.FirstOrDefault(x => x.Id == editedExpense.Id);
            // Если списание найдено.
            if (expense != null)
            {
                // Присвоить значения свойств отредактированного списания текущему.
                foreach (var prop in expense.GetType().GetProperties())
                {
                    prop.SetValue(expense, prop.GetValue(editedExpense));
                }
            }
            // Если отредактированное списание относится к текущему пользователю программы.
            if (CurrentUser.Id == editedExpense.User.Id)
            {
                // Найти списание, равное редактированному, в текущем списке списаний пользователя.
                ExpenseDTO userExpense = UserExpenses.FirstOrDefault(x => x.Id == editedExpense.Id);
                // Если списание найдено.
                if (userExpense != null)
                {
                    // Присвоить значения свойств отредактированного списания текущему.
                    foreach (var prop in userExpense.GetType().GetProperties())
                    {
                        prop.SetValue(userExpense, prop.GetValue(editedExpense));
                    }
                }
            }
        }

        /// <summary>
        /// Обновляет одно списание пользователя.
        /// </summary>
        /// <param name="editedExpense">Отредактированное списание пользователя</param>
        public void UpdateUserExpense(ExpenseDTO editedExpense)
        {
            // Найти списание, равное редактированному, в текущем списке списаний пользователя.
            ExpenseDTO userExpense = UserExpenses.FirstOrDefault(x => x.Id == editedExpense.Id);
            // Если списание найдено.
            if (userExpense != null)
            {
                // Присвоить значения свойств отредактированного списания текущему.
                foreach (var prop in userExpense.GetType().GetProperties())
                {
                    prop.SetValue(userExpense, prop.GetValue(editedExpense));
                }
            }
        }

        /// <summary>
        /// Обновляет одно поступление.
        /// </summary>
        /// <param name="editedReceipt">Отредактированное поступление</param>
        public void UpdateReceipt(ReceiptDTO editedReceipt)
        {
            // Найти поступление, равное редактированному, в текущем списке поступлений ОСП.
            ReceiptDTO receipt = Receipts.FirstOrDefault(x => x.Id == editedReceipt.Id);
            // Если поступление найдено.
            if (receipt != null)
            {
                // Присвоить значения свойств отредактированного поступления текущему.
                foreach (var prop in receipt.GetType().GetProperties())
                {
                    prop.SetValue(receipt, prop.GetValue(editedReceipt));
                }
            }
        }

        /// <summary>
        /// Обновляет одного поставщика.
        /// </summary>
        /// <param name="editedProvider">Отредактированный поставщик</param>
        public void UpdateProvider(ProviderDTO editedProvider)
        {
            // Найти поставщика, равного редактированному, в текущем списке поставщиков ОСП.
            ProviderDTO provider = Providers.FirstOrDefault(x => x.Id == editedProvider.Id);
            // Если поставщик найден.
            if (provider != null)
            {
                // Присвоить значения свойств отредактированного поставщика текущему.
                foreach (var prop in provider.GetType().GetProperties())
                {
                    prop.SetValue(provider, prop.GetValue(editedProvider));
                }
            }
        }

        /// <summary>
        /// Обновляет одну электронную почту.
        /// </summary>
        /// <param name="editedEmail">Отредактированная почта</param>
        public void UpdateEmail(EmailDTO editedEmail)
        {
            // Найти почту, равного редактированной, в текущем списке почты ОСП.
            EmailDTO email = Emails.FirstOrDefault(x => x.Id == editedEmail.Id);
            // Если почта найдена.
            if (email != null)
            {
                // Присвоить значения свойств отредактированной почты текущей.
                foreach (var prop in email.GetType().GetProperties())
                {
                    prop.SetValue(email, prop.GetValue(editedEmail));
                }
            }
        }

        /// <summary>
        /// Обновляет один картридж.
        /// </summary>
        /// <param name="editedCartridge">Отредактированный картридж</param>
        public void UpdateCartridge(CartridgeDTO editedCartridge)
        {
            // Найти картридж, равный редактированному, в текущем списке всех картриджей.
            CartridgeDTO cartridge = Cartridges.FirstOrDefault(x => x.Id == editedCartridge.Id);
            // Найти баланс картриджа, равный редактированному, в текущем списке баланса ОСП.
            BalanceDTO balance = Balance.FirstOrDefault(x => x.Cartridge.Id == editedCartridge.Id);
            // Найти все списания пользователя ОСП, в которых списан изменяемый картридж.
            IEnumerable<ExpenseDTO> userExpenses = UserExpenses.Where(x => x.Cartridge.Id == editedCartridge.Id);
            // Найти все списания ОСП, в которых списан изменяемый картридж.
            IEnumerable<ExpenseDTO> expenses = Expenses.Where(x => x.Cartridge.Id == editedCartridge.Id);
            // Найти все поступления ОСП, в которых добавлен изменяемый картридж.
            IEnumerable<ReceiptDTO> receipts = Receipts.Where(x => x.Cartridges.Where(c => c.Cartridge.Id == cartridge.Id).Any());

            // Если картридж найден.
            if (cartridge != null)
            {
                // Присвоить значения свойств отредактированного картриджа текущему.
                foreach (var prop in cartridge.GetType().GetProperties())
                {
                    prop.SetValue(cartridge, prop.GetValue(editedCartridge));
                }
                // Обновить картридж в объектах списаний, поступлений и баланса.
                if (balance != null)
                {
                    balance.Cartridge = cartridge;
                }
                if (expenses != null)
                {
                    foreach (var expense in expenses)
                    {
                        expense.Cartridge = cartridge;
                    }
                }
                if (userExpenses != null)
                {
                    foreach (var userExpense in userExpenses)
                    {
                        userExpense.Cartridge = cartridge;
                    }
                }
                if (receipts != null)
                {
                    foreach (var receipt in receipts)
                    {
                        ReceiptCartridgeDTO receiptCartridge = receipt.Cartridges.FirstOrDefault(x => x.Cartridge.Id == cartridge.Id);
                        receiptCartridge.Cartridge = cartridge;
                    }
                }
            }
        }

        /// <summary>
        /// Обновляет один принтер.
        /// </summary>
        /// <param name="editedPrinter">Отредактированный принтер</param>
        public void UpdatePrinter(PrinterDTO editedPrinter)
        {
            // Найти принтер, равный редактированному, в текущем списке всех принтеров.
            PrinterDTO printer = Printers.FirstOrDefault(x => x.Id == editedPrinter.Id);
            // Если принтер найден.
            if (printer != null)
            {
                // Присвоить значения свойств отредактированного картриджа текущему.
                foreach (var prop in printer.GetType().GetProperties())
                {
                    prop.SetValue(printer, prop.GetValue(editedPrinter));
                }
            }
        }

        /// <summary>
        /// Обновляет одно ОСП.
        /// </summary>
        /// <param name="editedOsp">Отредактированное ОСП</param>
        public void UpdateOsp(OspDTO editedOsp)
        {
            // Найти ОСП, равное редактированному, в текущем списке всех ОСП.
            OspDTO osp = Osps.FirstOrDefault(x => x.Id == editedOsp.Id);
            // Если ОСП найден.
            if (osp != null)
            {
                // Присвоить значения свойств отредактированного ОСП текущему.
                foreach (var prop in osp.GetType().GetProperties())
                {
                    prop.SetValue(osp, prop.GetValue(editedOsp));
                }
                // Если отредактированное ОСП является текущиим для пользователя.
                if (editedOsp.Id == CurrentUser.Osp.Id)
                {
                    // Присвоить значения свойств отредактированного ОСП текущему ОСП пользователя.
                    foreach (var prop in CurrentUser.Osp.GetType().GetProperties())
                    {
                        prop.SetValue(CurrentUser.Osp, prop.GetValue(editedOsp));
                    }
                }
            }
        }

        /// <summary>
        /// Обновляет одного пользователя.
        /// </summary>
        /// <param name="editedOsp">Отредактированный пользователь</param>
        public void UpdateUser(UserDTO editedUser)
        {
            // Найти пользователя, равное редактированному, в текущем списке всех пользователей.
            UserDTO user = Users.FirstOrDefault(x => x.Id == editedUser.Id);
            // Если пользователь найден.
            if (user != null)
            {
                // Присвоить значения свойств отредактированного пользователя текущему.
                foreach (var prop in user.GetType().GetProperties())
                {
                    prop.SetValue(user, prop.GetValue(editedUser));
                }
                // Если отредактированный пользователь является текущим пользователем.
                if (editedUser.Id == CurrentUser.Id)
                {
                    // Присвоить значения свойств отредактированного пользователя текущему пользователю.
                    foreach (var prop in CurrentUser.GetType().GetProperties())
                    {
                        prop.SetValue(CurrentUser, prop.GetValue(editedUser));
                    }
                }
            }
        }


        /// <summary>
        /// Добавляет новое списание пользователя.
        /// </summary>
        /// <param name="newExpense">Новое списание пользователя</param>
        public void AddNewUserExpense(ExpenseDTO newExpense)
        {
            if (newExpense != null)
            {
                UserExpenses.Add(newExpense);
            }
        }

        /// <summary>
        /// Добавляет новое списение ОСП.
        /// </summary>
        /// <param name="newExpense">Новое списание ОСП</param>
        public void AddNewOspExpense(ExpenseDTO newExpense)
        {
            if (newExpense != null)
            {
                Expenses.Add(newExpense);
            }
        }

        /// <summary>
        /// Добавляет новое поступление ОСП.
        /// </summary>
        /// <param name="newReceipt">Новое поступление ОСП</param>
        public void AddNewOspReceipt(ReceiptDTO newReceipt)
        {
            if (newReceipt != null)
            {
                Receipts.Add(newReceipt);
            }
        }

        /// <summary>
        /// Добавляет нового поставщика ОСП.
        /// </summary>
        /// <param name="newProvider">Новый поставщик ОСП</param>
        public void AddNewOspProvider(ProviderDTO newProvider)
        {
            if (newProvider != null)
            {
                Providers.Add(newProvider);
                //OspProvidersUpdated?.Invoke();
            }
        }

        /// <summary>
        /// Добавляет новую почту ОСП.
        /// </summary>
        /// <param name="newEmail">Новая почта ОСП</param>
        public void AddNewOspEmail(EmailDTO newEmail)
        {
            if (newEmail != null)
            {
                Emails.Add(newEmail);
            }
        }

        /// <summary>
        /// Добавляет новый картридж в общий список.
        /// </summary>
        /// <param name="newCartridge">Новый картридж</param>
        public void AddNewCartridge(CartridgeDTO newCartridge)
        {
            if (newCartridge != null)
            {
                Cartridges.Add(newCartridge);
            }
        }

        /// <summary>
        /// Добавляет новый принтер в общий список.
        /// </summary>
        /// <param name="newPrinter">Новый принтер</param>
        public void AddNewPrinter(PrinterDTO newPrinter)
        {
            if (newPrinter != null)
            {
                Printers.Add(newPrinter);
            }
        }

        /// <summary>
        /// Добавляет новое ОСП в общий список.
        /// </summary>
        /// <param name="newOsp">Новое ОСП</param>
        public void AddNewOsp(OspDTO newOsp)
        {
            if (newOsp != null)
            {
                Osps.Add(newOsp);
            }
        }

        /// <summary>
        /// Добавляет нового пользователя в общий список.
        /// </summary>
        /// <param name="newUser">Новый пользователь</param>
        public void AddNewUser(UserDTO newUser)
        {
            if (newUser != null)
            {
                Users.Add(newUser);
            }
        }
    }
}
