﻿
namespace Anycmd.Engine.Host.Ac.MessageHandlers
{
    using Commands;
    using Engine.Ac;
    using Engine.Ac.Messages.Identity;
    using Exceptions;
    using Identity;
    using Repositories;
    using System.Linq;

    public class UpdateAccountCommandHandler : CommandHandler<UpdateAccountCommand>
    {
        private readonly IAcDomain _host;

        public UpdateAccountCommandHandler(IAcDomain host)
        {
            this._host = host;
        }

        public override void Handle(UpdateAccountCommand command)
        {
            var accountRepository = _host.RetrieveRequiredService<IRepository<Account>>();
            if (accountRepository.AsQueryable().Any(a => a.Code == command.Input.Code && a.Id != command.Input.Id))
            {
                throw new ValidationException("用户编码重复");
            }
            var entity = accountRepository.GetByKey(command.Input.Id);
            if (entity == null)
            {
                throw new NotExistException();
            }
            AccountState devAccount;
            if (_host.SysUserSet.TryGetDevAccount(entity.Id, out devAccount))
            {
                if (!command.UserSession.IsDeveloper())
                {
                    throw new AnycmdException("对不起，您不能修改开发者的信息");
                }
                if (command.UserSession.Account.Id != command.Input.Id)
                {
                    throw new ValidationException("对不起，您不能修改别的开发者的信息");
                }
            }
            if (command.Input.CatalogCode != entity.CatalogCode)
            {
                if (string.IsNullOrEmpty(command.Input.CatalogCode))
                {
                    throw new AnycmdException("用户必须属于一个目录");
                }
                CatalogState catalog;
                if (!_host.CatalogSet.TryGetCatalog(command.Input.CatalogCode, out catalog))
                {
                    throw new AnycmdException("意外的目录码" + command.Input.CatalogCode);
                }
            }
            entity.Update(command.Input);
            accountRepository.Update(entity);
            accountRepository.Context.Commit();
            if (_host.SysUserSet.TryGetDevAccount(entity.Id, out devAccount))
            {
                _host.EventBus.Publish(new DeveloperUpdatedEvent(command.UserSession, entity));
            }
            _host.EventBus.Publish(new AccountUpdatedEvent(command.UserSession, entity));
            _host.EventBus.Commit();
        }
    }
}
