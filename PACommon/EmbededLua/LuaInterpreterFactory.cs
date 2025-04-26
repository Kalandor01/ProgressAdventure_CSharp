namespace PACommon.EmbededLua
{
    /// <summary>
    /// Class to help create a <see cref="LuaInterpreter"/> object.
    /// </summary>
    public class LuaInterpreterFactory
    {
        /// <summary>
        /// Creates a <see cref="LuaInterpreter"/> object that has access to basic lua methods/classes like "math", "string", ("os") and others.
        /// </summary>
        /// <param name="envWhitelist">The extra enviroment whitelist values, on top of the default one.</param>
        /// <param name="sandboxEnvVars">Same as <paramref name="enviromentVars"/>, but accesable in the sandbox.</param>
        /// <inheritdoc cref="LuaInterpreter(List{Type}, Dictionary{string, string}, Dictionary{string, object?}, string)"/>
        /// <returns>The created <see cref="LuaInterpreter"/> object or null, if the setup failed.</returns>
        public static LuaInterpreter? SetupRunLuaCode(
            List<Type>? importedNamespaces = null,
            Dictionary<string, string>? envWhitelist = null,
            Dictionary<string, object?>? enviromentVars = null,
            Dictionary<string, object?>? sandboxEnvVars = null,
            string extraSetupCode = ""
        )
        {
            var defaultEnvWhitelist = new Dictionary<string, string>()
            {
                ["assert"] = "assert",
                ["error"] = "error",
                ["ipairs"] = "ipairs",
                ["next"] = "next",
                ["pairs"] = "pairs",
                ["pcall"] = "pcall",
                ["print"] = "print",
                ["select"] = "select",
                ["tonumber"] = "tonumber",
                ["tostring"] = "tostring",
                ["type"] = "type",
                ["unpack"] = "unpack",
                ["_VERSION"] = "_VERSION",
                ["xpcall"] = "xpcall",

                ["coroutine"] = "coroutine", //coroutine.yield: SAFE(probably) - assuming caller handles this
                ["string"] = "string",
                //string.dump, --UNSAFE(potentially) - allows seeing implementation of functions
                //string.find, --warning: a number of functions like this can still lock up the CPU[6]
                ["table"] = "table",
                ["math"] = "math",
                //math.random, --SAFE(mostly) - but note that returned numbers are pseudorandom, and calls to this function affect subsequent calls.This may have statistical implications.
                //math.randomseed, --UNSAFE(maybe) - see math.random

                ["os_clock"] = "os.clock",
                ["os_date"] = "os.date", //UNSAFE - This can crash on some platforms(undocumented).For example, os.date'%v'.It is reported that this will be fixed in 5.2 or 5.1.3.

                ["os_difftime"] = "os.difftime",
                ["os_getenv"] = "os.getenv", //UNSAFE(potentially) - depending on what environment variables contain
                ["os_time"] = "os.time",
                ["os_tmpname"] = "os.tmpname", //UNSAFE(maybe) - only in that it provides some information on the structure of the file system
            };

            if (envWhitelist is not null)
            {
                foreach (var envValue in envWhitelist)
                {
                    defaultEnvWhitelist[envValue.Key] = envValue.Value;
                }
            }

            var actualEnvVars = new Dictionary<string, object?>();
            if (enviromentVars is not null)
            {
                foreach (var enviromentVar in enviromentVars)
                {
                    actualEnvVars[enviromentVar.Key] = enviromentVar.Value;
                }
            }

            if (sandboxEnvVars is not null)
            {
                foreach (var sandboxValue in sandboxEnvVars)
                {
                    actualEnvVars[$"__{sandboxValue.Key}_sandbox_value"] = sandboxValue.Value;
                }

                foreach (var sandboxValue in sandboxEnvVars)
                {
                    defaultEnvWhitelist[sandboxValue.Key] = $"__{sandboxValue.Key}_sandbox_value";
                }
            }

            var interpreter = new LuaInterpreter(
                importedNamespaces ?? [],
                defaultEnvWhitelist,
                actualEnvVars,
                extraSetupCode
            );
            return interpreter.IsOk ? interpreter : null;
        }
    }
}
