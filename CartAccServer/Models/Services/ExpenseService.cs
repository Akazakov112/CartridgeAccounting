using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CartAccLibrary.Dto;
using CartAccLibrary.Entities;
using CartAccServer.Models.Infrastructure;
using CartAccServer.Models.Interfaces.Repository;
using CartAccServer.Models.Interfaces.Services;

namespace CartAccServer.Models.Services
{
    /// <summary>
    /// Сервис работы со списаниями.
    /// </summary>
    public class ExpenseService : IEntityService<Expense, ExpenseDTO>
    {
        private IUnitOfWork Database { get; }

        public ExpenseService(IUnitOfWork unitOfWork)
        {
            Database = unitOfWork;
        }


        public ICollection<ExpenseDTO> GetAll()
        {
            IEnumerable<Expense> expenses = Database.Expenses.GetAll();
            // Если списаия не найдены.
            if (expenses is null)
            {
                throw new ValidationException("Списания не получены", "");
            }
            // Создать список списаний Dto.
            var expensesDto = expenses.Select(p => new ExpenseDTO()
            {
                Id = p.Id,
                Number = p.Number,
                Basis = p.Basis,
                Cartridge = new CartridgeDTO(p.Cartridge.Id, p.Cartridge.Model, new ObservableCollection<PrinterDTO>(), p.Cartridge.InUse),
                Count = p.Count,
                Date = p.Date,
                Delete = p.Delete,
                Edit = p.Edit,
                User = new UserDTO(p.User.Id, p.User.Login, p.User.Fullname, new OspDTO(), new AccessDTO(), p.User.Active),
                OspId = p.Osp.Id
            });
            // Вернуть DTO списаний.
            return new ObservableCollection<ExpenseDTO>(expensesDto);
        }

        public ExpenseDTO Get(int id)
        {
            // Получить списание из бд.
            Expense expense = Database.Expenses.Get(id);
            // Если списание не найдено.
            if (expense is null)
            {
                throw new ValidationException("Списание не получено", "");
            }
            // Создать списание Dto.
            var expenseDto = new ExpenseDTO()
            {
                Id = expense.Id,
                Number = expense.Number,
                Basis = expense.Basis,
                Cartridge = new CartridgeDTO(expense.Cartridge.Id, expense.Cartridge.Model, new ObservableCollection<PrinterDTO>(), expense.Cartridge.InUse),
                Count = expense.Count,
                Date = expense.Date,
                Delete = expense.Delete,
                Edit = expense.Edit,
                User = new UserDTO(expense.User.Id, expense.User.Login, expense.User.Fullname, new OspDTO(), new AccessDTO(), expense.User.Active),
                OspId = expense.Osp.Id
            };
            // Вернуть DTO списание.
            return expenseDto;
        }

        public ICollection<ExpenseDTO> Find(Func<Expense, bool> predicate)
        {
            IEnumerable<Expense> expenses = Database.Expenses.Find(predicate);
            // Если списания не найдены.
            if (expenses is null)
            {
                throw new ValidationException("Списания не получены", "");
            }
            // Создать список списаний Dto.
            var expensesDto = expenses.Select(p => new ExpenseDTO()
            {
                Id = p.Id,
                Number = p.Number,
                Basis = p.Basis,
                Cartridge = new CartridgeDTO(p.Cartridge.Id, p.Cartridge.Model, new ObservableCollection<PrinterDTO>(), p.Cartridge.InUse),
                Count = p.Count,
                Date = p.Date,
                Delete = p.Delete,
                Edit = p.Edit,
                User = new UserDTO(p.User.Id, p.User.Login, p.User.Fullname, new OspDTO(), new AccessDTO(), p.User.Active),
                OspId = p.Osp.Id
            });
            // Вернуть DTO списаний.
            return new ObservableCollection<ExpenseDTO>(expensesDto);
        }

        public void Add(ExpenseDTO item)
        {
            // Найти в бд связанные сущности для списания.
            Cartridge cart = Database.Cartridges.Get(item.Cartridge.Id);
            User user = Database.Users.Get(item.User.Id);
            Osp osp = Database.Osps.Get(item.OspId);
            // Найти баланс ОСП с картриджем списания.
            Balance balance = Database.Balances.Find(x => x.Osp.Id == item.OspId && x.Cartridge.Id == item.Cartridge.Id).FirstOrDefault();
            // Вычесть из баланса количество картриджей в списании.
            balance.Count -= item.Count;
            // Найти последнее списание в ОСП.
            Expense lastExpense = Database.Expenses.Find(x => x.Osp.Id == item.OspId).LastOrDefault();
            // Создать списание по данным DTO.
            Expense newExpense = new Expense()
            {
                Basis = item.Basis,
                Count = item.Count,
                Date = item.Date,
                Delete = item.Delete,
                Edit = item.Edit,
                Number = lastExpense is null ? 1 : lastExpense.Number + 1,
                Cartridge = cart,
                User = user,
                Osp = osp
            };
            // Добавить созданное списание в бд.
            Database.Expenses.Create(newExpense);
            // Обновить баланс в БД.
            Database.Balances.Update(balance);
            // Сохранить изменения.
            Database.Save();
        }

        public void Update(ExpenseDTO item)
        {
            // Найти списание в бд по Id.
            Expense dbExpense = Database.Expenses.Get(item.Id);
            // Найти баланс для картриджа редактируемого списания.
            Balance balance = Database.Balances.Find(x => x.Osp.Id == item.OspId && x.Cartridge.Id == item.Cartridge.Id).FirstOrDefault();
            // Если была установлена пометка удаления.
            if (item.Delete == true && dbExpense.Delete == false)
            {
                balance.Count += item.Count;
                dbExpense.Delete = item.Delete;
            }
            // Елси пометка удаления была снята.
            else if (item.Delete == false && dbExpense.Delete == true)
            {
                balance.Count -= item.Count;
                dbExpense.Delete = item.Delete;
            }
            // Если метка удаления не менялась.
            else
            {
                // Если изменился картридж.
                if (item.Cartridge.Id != dbExpense.Cartridge.Id)
                {
                    // Найти баланс для старого картриджа списания.
                    Balance oldCartDbBalance = Database.Balances.Find(x => x.Osp.Id == dbExpense.Osp.Id && x.Cartridge.Id == dbExpense.Cartridge.Id).FirstOrDefault();
                    // Вернуть количество старого картриджа в баланс.
                    oldCartDbBalance.Count += dbExpense.Count;
                    // Списать из баланса нового картриджа количество списания.
                    balance.Count -= item.Count;
                    // Обновить значение баланса старого картриджа в бд.
                    Database.Balances.Update(oldCartDbBalance);
                    // Найти картридж из измененного списания.
                    Cartridge cart = Database.Cartridges.Get(item.Cartridge.Id);
                    // Изменить картридж в списании.
                    dbExpense.Cartridge = cart;
                }
                // Если картридж не менялся.
                else
                {
                    // Если количество увеличилось.
                    if (item.Count > dbExpense.Count)
                    {
                        // Вычесть из баланса разницу между новым и старым значениями списаний.
                        balance.Count -= item.Count - dbExpense.Count;
                    }
                    // Если количество уменьшилось
                    else if (item.Count < dbExpense.Count)
                    {
                        // Прибавить в баланс разницу между старым и новым значениями списаний.
                        balance.Count += dbExpense.Count - item.Count;
                    }
                }
                // Изменить количество списанных картриджей.
                dbExpense.Count = item.Count;
            }
            // Изменить основание списания.
            dbExpense.Basis = item.Basis;
            // Обновить значение списания в бд.
            Database.Expenses.Update(dbExpense);
            // Обновить значение баланса в бд.
            Database.Balances.Update(balance);
            // Сохранить изменения.
            Database.Save();
        }
    }
}
