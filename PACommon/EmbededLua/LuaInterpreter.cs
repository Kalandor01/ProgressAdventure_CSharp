using NLua;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PACommon.EmbededLua
{
    /// <summary>
    /// Class for running lua code in a sandboxed enviroment.
    /// </summary>
    public class LuaInterpreter
    {
        /// <summary>
        /// The lua function that can run lua code in a sandboxed enviroment.
        /// </summary>
        private readonly LuaFunction? _runLuaCodeFunction = null;

        /// <summary>
        /// If the sandboxed enviroment was successfuly set up.
        /// </summary>
        public bool IsOk { get => _runLuaCodeFunction is not null; }

        /// <summary>
        /// <inheritdoc cref="LuaInterpreter" path="//summary"/>
        /// </summary>
        /// <param name="importedNamespaces">A list of C# namespaces to be accesable in the setup code and optionaly in the sandbox.</param>
        /// <param name="envWhitelist">The list of classes, functions and variables to be accesable in the sandbox.</param>
        /// <param name="enviromentVars">The list of enviroment variables to be accesable to the setup code.</param>
        /// <param name="extraSetupCode">Extra setup code to run before the sandbox is created.</param>
        public LuaInterpreter(
            List<Type> importedNamespaces,
            Dictionary<string, string> envWhitelist,
            Dictionary<string, object?> enviromentVars,
            string extraSetupCode
        )
        {
            var lua = new Lua
            {
                MaximumRecursion = 10
            };

            var h = typeof(Lua);

            lua.State.Encoding = Encoding.UTF8;
            if (importedNamespaces.Count > 0)
            {
                lua.LoadCLRPackage();
            }

            var setupCode = $@"
                successfullSetup = false

                {string.Join('\n', importedNamespaces.Select(n => $"import ('{n.Assembly.GetName().Name}', '{n.Namespace}')"))}

                {extraSetupCode}

		        env = {{ {string.Join(", ", envWhitelist.Select(e => $"[\"{e.Key}\"] = {e.Value}"))} }} -- add functions you know are safe here

                function RunUntrustedCode(untrusted_code)
                    local untrusted_function, message = load(untrusted_code, nil, 't', env)
                    if not untrusted_function then return nil, message end
                    return pcall(untrusted_function)
                end

                successfullSetup = true
            ";

            foreach (var ev in enviromentVars)
            {
                lua[ev.Key] = ev.Value;
            }

            try
            {
                lua.DoString(setupCode);
            }
            catch
            {
                return;
            }

            var success = lua["successfullSetup"] as bool? ?? false;
            if (!success)
            {
                return;
            }

            _runLuaCodeFunction = lua["RunUntrustedCode"] as LuaFunction;
        }

        /// <summary>
        /// Runs a lua code.
        /// </summary>
        /// <param name="code">The lua code to run.</param>
        /// <param name="returnValues">The list of returned values from the code that was run, or the string representation of the error, if the code failed.</param>
        /// <returns>If te code was succesfuly run.</returns>
        public bool TryRunCode(string code, [NotNullWhen(true)] out object[]? returnValues)
        {
            returnValues = null;
            if (_runLuaCodeFunction is null)
            {
                return false;
            }

            var returnValuesAll = _runLuaCodeFunction.Call(code);
            returnValues = returnValuesAll[1..];
            return returnValuesAll.Length != 0 && returnValuesAll[0] is bool executeSuccess && executeSuccess;
        }
    }
}
