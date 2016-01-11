# mibus
Another .NET Micro Bus/Mediator Library, enables clean separation of concern when writing C# applications.

==Usage==

private readonly IMediator _mediator = <IoCContainer>.Resolve<IMediator>();
List<User> userList = _medator.Execute(new AllUsersQuery());

...

==Detailed example==

Let's say I wanted to have all commands for interacting with a User object, available from one place only in
my entire application, I will simply create a single file with the following class definition:

  public class UserCommands
    {
        #region AllUsersQuery
        public class AllUsersQuery : IQuery<IEnumerable<User>>
        {
        }

        public class Handler : IQueryHandler<AllUsersQuery, IEnumerable<User>>
        {
            public Handler()
            {
            }

            public IEnumerable<User> Handle(AllUsersQuery allRoomListQuery)
            {
                using (var cxt = new CabanaEntities())
                {
                    return cxt.Users.ToList().AsEnumerable();
                }
            }
        }
        #endregion

        #region Save Command
        public class Save : ICommand
        {
            public User User { get; set; }
        }

        public class UserSavedEvent : IEvent
        {
            public User User { get; set; }
        }

        public class SaveHandler : ICommandHandler<Save>
        {
            public void Execute(Save command)
            {
                using (var cxt = new CabanaEntities())
                {
                    if (command.User.UserID == 0)
                    {
                        cxt.Users.Add(command.User);
                    }
                    else
                    {
                        cxt.Users.Attach(command.User);
                    }
                    
                    cxt.SaveChanges();
                }
            }
        }
        #endregion

        #region Delete Command
        public class Delete : ICommand
        {
            public User User { get; set; }
        }

        public class DeleteHandler : ICommandHandler<Delete>
        {
            public void Execute(Delete command)
            {
                using (var cxt = new CabanaEntities())
                {
                    cxt.Users.Attach(command.User);
                    cxt.SaveChanges();
                }
            }
        }
        #endregion
    }
    
    And make calls to it using the mediator like this:
    
    public class UserForm: Form {
        private readonly IMediator _mediator = <IoCContainer>.Resolve<IMediator>();
        
        private void Form_Load(object sender, EventArgs e){
            List<User> userList = _medator.Execute(new AllUsersQuery());
            UserGrid.DataSource = userList;
        }
    }
    
    
    
