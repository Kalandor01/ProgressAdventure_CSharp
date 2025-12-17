using System.Diagnostics.CodeAnalysis;
using Lua;
using Lua.Platforms;
using Lua.Standard;

namespace PACommon.EmbededLua
{
    /// <summary>
    /// Class for running lua code in a sandboxed enviroment.
    /// </summary>
    public class LuaInterpreter
    {
        private readonly LuaState? _luaState = null;
        
        /// <summary>
        /// The lua function that can run lua code in a sandboxed enviroment.
        /// </summary>
        private readonly LuaFunction? _runLuaCodeFunction = null;

        /// <summary>
        /// If the sandboxed enviroment was successfuly set up.
        /// </summary>
        public bool IsOk => _runLuaCodeFunction is not null;

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="LuaInterpreter" path="//summary"/>
        /// </summary>
        /// <param name="envWhitelist">The list of classes, functions and variables to be accesable in the sandbox.</param>
        /// <param name="enviromentVars">The list of enviroment variables to be accesable to the setup code.</param>
        /// <param name="extraSetupCode">Extra setup code to run before the sandbox is created.</param>
        public LuaInterpreter(
            Dictionary<string, string> envWhitelist,
            Dictionary<string, LuaValue> enviromentVars,
            string extraSetupCode
        )
        {
            _luaState = LuaState.Create(LuaPlatform.Default);
            _luaState.OpenStandardLibraries();
            
            var setupCode = $@"
                successfullSetup = false

                {extraSetupCode}

		        env = {{ {string.Join(", ", envWhitelist.Select(e => $"[\"{e.Key}\"] = {e.Value}"))} }}

                function RunUntrustedCode(untrusted_code)
                    local untrusted_function, message = load(untrusted_code, nil, 't', env)
                    if not untrusted_function then return nil, message end
                    return pcall(untrusted_function)
                end

                successfullSetup = true
            ";

            foreach (var ev in enviromentVars)
            {
                _luaState.Environment[ev.Key] = ev.Value;
            }

            try
            {
                _luaState.DoStringAsync(setupCode).AsTask().Wait();
            }
            catch
            {
                return;
            }

            var success = _luaState.Environment["successfullSetup"].TryRead(out bool resS) && resS;
            if (!success)
            {
                return;
            }

            if (_luaState.Environment["RunUntrustedCode"].TryRead(out LuaFunction function))
            {
                _runLuaCodeFunction = function;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Runs a lua code.
        /// </summary>
        /// <param name="code">The lua code to run.</param>
        /// <param name="returnValues">The list of returned values from the code that was run, or the string representation of the error, if the code failed.</param>
        /// <returns>If te code was succesfuly run.</returns>
        public bool TryRunCode(string code, [NotNullWhen(true)] out LuaValue[]? returnValues)
        {
            returnValues = null;
            if (_runLuaCodeFunction is null)
            {
                return false;
            }

            var returnValuesAll = CallFunction(_runLuaCodeFunction, code);
            if (returnValuesAll is null)
            {
                return false;
            }
            
            returnValues = returnValuesAll[1..];
            return returnValuesAll.Length != 0 && returnValuesAll[0].TryRead(out bool res) && res;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Runs a lua function that has return values.
        /// </summary>
        /// <param name="function">The function to run.</param>
        /// <param name="args">The arguments to the function.</param>
        /// <returns>The return values, or null if the function wasn't run.</returns>
        private LuaValue[]? CallFunction(LuaFunction function, params ReadOnlySpan<LuaValue> args)
        {
            if (_luaState is null)
            {
                return null;
            }

            if (args.Length > 0)
            {
                _luaState.Stack.PushRange(args);
            }
            var task = _luaState.RunAsync(function, args.Length).AsTask();
            task.Wait();
            var returnNum = task.Result;
            var results = _luaState.ReadStack(returnNum);
            var res = results.AsSpan().ToArray();
            results.Dispose();
            return res;
        }

        /// <summary>
        /// Runs a lua function that doesn't have return values.
        /// </summary>
        /// <param name="function">The function to run.</param>
        /// <param name="args">The arguments to the function.</param>
        /// <returns>If the function was run.</returns>
        private bool CallFunctionNoReturn(LuaFunction function, params ReadOnlySpan<LuaValue> args)
        {
            if (_luaState is null)
            {
                return false;
            }

            if (args.Length > 0)
            {
                _luaState.Stack.PushRange(args);
            }
            _luaState.RunAsync(function, args.Length).AsTask().Wait();
            return true;
        }
        #endregion
    }
}
