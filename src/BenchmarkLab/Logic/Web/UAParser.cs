﻿// https://github.com/tobie/ua-parser

#region Apache License, Version 2.0
// 
// Copyright 2014 Atif Aziz
// Portions Copyright 2012 Søren Enemærke
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

namespace UAParser
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    #endregion

    public sealed class Device
    {
        public Device(string family, bool isSpider)
        {
            Family = family;
            IsSpider = isSpider;
        }

        public string Family
        {
            get; private set;
        }
        public bool IsSpider
        {
            get; private set;
        }

        public override string ToString()
        {
            return Family;
        }
    }

    // ReSharper disable once InconsistentNaming
    public sealed class OS
    {
        public OS(string family, string major, string minor, string patch, string patchMinor)
        {
            Family = family;
            Major = major;
            Minor = minor;
            Patch = patch;
            PatchMinor = patchMinor;
        }

        public string Family
        {
            get; private set;
        }
        public string Major
        {
            get; private set;
        }
        public string Minor
        {
            get; private set;
        }
        public string Patch
        {
            get; private set;
        }
        public string PatchMinor
        {
            get; private set;
        }

        public override string ToString()
        {
            var version = VersionString.Format(Major, Minor, Patch, PatchMinor);
            return Family + (!string.IsNullOrEmpty(version) ? " " + version : null);
        }
    }

    public sealed class UserAgent
    {
        public UserAgent(string family, string major, string minor, string patch)
        {
            Family = family;
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public string Family
        {
            get; private set;
        }
        public string Major
        {
            get; private set;
        }
        public string Minor
        {
            get; private set;
        }
        public string Patch
        {
            get; private set;
        }

        public override string ToString()
        {
            var version = VersionString.Format(Major, Minor, Patch);
            return Family + (!string.IsNullOrEmpty(version) ? " " + version : null);
        }
    }

    static class VersionString
    {
        public static string Format(params string[] parts)
        {
            return string.Join(".", parts.Where(v => !String.IsNullOrEmpty(v)).ToArray());
        }
    }

    public class ClientInfo
    {
        // ReSharper disable once InconsistentNaming
        public OS OS
        {
            get; private set;
        }
        public Device Device
        {
            get; private set;
        }
        public UserAgent UserAgent
        {
            get; private set;
        }

        public ClientInfo(OS os, Device device, UserAgent userAgent)
        {
            OS = os;
            Device = device;
            UserAgent = userAgent;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", OS, Device, UserAgent);
        }
    }

    public sealed class Parser
    {
        readonly Func<string, OS> _osParser;
        readonly Func<string, Device> _deviceParser;
        readonly Func<string, UserAgent> _userAgentParser;

        Parser(MinimalYamlParser yamlParser)
        {
            const string other = "Other";
            var defaultDevice = new Device(other, isSpider: false);

            _userAgentParser = CreateParser(Read(yamlParser.ReadMapping("user_agent_parsers"), Config.UserAgent), new UserAgent(other, null, null, null));
            _osParser = CreateParser(Read(yamlParser.ReadMapping("os_parsers"), Config.OS), new OS(other, null, null, null, null));
            _deviceParser = CreateParser(Read(yamlParser.ReadMapping("device_parsers"), Config.Device), defaultDevice.Family, f => defaultDevice.Family == f ? defaultDevice : new Device(f, "Spider".Equals(f, StringComparison.OrdinalIgnoreCase)));
        }

        static IEnumerable<T> Read<T>(IEnumerable<Dictionary<string, string>> entries, Func<Func<string, string>, T> selector)
        {
            return from cm in entries select selector(cm.Find);
        }

        public static Parser FromYaml(string yaml)
        {
            return new Parser(new MinimalYamlParser(yaml));
        }
        public static Parser FromYamlFile(string path)
        {
            return new Parser(new MinimalYamlParser(File.ReadAllText(path)));
        }

        public static Parser GetDefault()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return new Parser(new MinimalYamlParser(File.ReadAllText("Regexes.yaml")));
        }

        public ClientInfo Parse(string uaString)
        {
            var os = ParseOS(uaString);
            var device = ParseDevice(uaString);
            var ua = ParseUserAgent(uaString);
            return new ClientInfo(os, device, ua);
        }

        public OS ParseOS(string uaString)
        {
            return _osParser(uaString);
        }
        public Device ParseDevice(string uaString)
        {
            return _deviceParser(uaString);
        }
        public UserAgent ParseUserAgent(string uaString)
        {
            return _userAgentParser(uaString);
        }

        static Func<string, T> CreateParser<T>(IEnumerable<Func<string, T>> parsers, T defaultValue) where T : class
        {
            return CreateParser(parsers, defaultValue, t => t);
        }

        static Func<string, TResult> CreateParser<T, TResult>(IEnumerable<Func<string, T>> parsers, T defaultValue, Func<T, TResult> selector) where T : class
        {
            parsers = parsers != null ? parsers.ToArray() : Enumerable.Empty<Func<string, T>>();
            return ua => selector(parsers.Select(p => p(ua)).FirstOrDefault(m => m != null) ?? defaultValue);
        }

        static class Config
        {
            // ReSharper disable once InconsistentNaming
            public static Func<string, OS> OS(Func<string, string> indexer)
            {
                var regex = Regex(indexer, "OS");
                var os = indexer("os_replacement");
                var v1 = indexer("os_v1_replacement");
                var v2 = indexer("os_v2_replacement");
                return Parsers.OS(regex, os, v1, v2);
            }

            public static Func<string, UserAgent> UserAgent(Func<string, string> indexer)
            {
                var regex = Regex(indexer, "User agent");
                var family = indexer("family_replacement");
                var v1 = indexer("v1_replacement");
                var v2 = indexer("v2_replacement");
                return Parsers.UserAgent(regex, family, v1, v2);
            }

            public static Func<string, string> Device(Func<string, string> indexer)
            {
                return Parsers.Device(Regex(indexer, "Device"), indexer("device_replacement"));
            }

            static Regex Regex(Func<string, string> indexer, string key)
            {
                var pattern = indexer("regex");
                if (pattern == null)
                    throw new Exception(String.Format("{0} is missing regular expression specification.", key));

                // Some expressions in the regex.yaml file causes parsing errors 
                // in .NET such as the \_ token so need to alter them before 
                // proceeding.

                if (pattern.IndexOf(@"\_", StringComparison.Ordinal) >= 0)
                    pattern = pattern.Replace(@"\_", "_");

                // TODO: potentially allow parser to specify e.g. to use 
                // compiled regular expressions which are faster but increase 
                // startup time

                return new Regex(pattern);
            }
        }

        static class Parsers
        {
            // ReSharper disable once InconsistentNaming
            public static Func<string, OS> OS(Regex regex, string osReplacement, string v1Replacement, string v2Replacement)
            {
                return Create(regex, from family in Replace(osReplacement, "$1")
                                     from v1 in Replace(v1Replacement)
                                     from v2 in Replace(v2Replacement)
                                     from v3 in Select(v => v)
                                     from v4 in Select(v => v)
                                     select new OS(family, v1, v2, v3, v4));
            }

            public static Func<string, string> Device(Regex regex, string familyReplacement)
            {
                return Create(regex, Replace(familyReplacement, "$1"));
            }

            public static Func<string, UserAgent> UserAgent(Regex regex, string familyReplacement, string majorReplacement, string minorReplacement)
            {
                return Create(regex, from family in Replace(familyReplacement, "$1")
                                     from v1 in Replace(majorReplacement)
                                     from v2 in Replace(minorReplacement)
                                     from v3 in Select()
                                     select new UserAgent(family, v1, v2, v3));
            }

            static Func<Match, IEnumerator<int>, string> Replace(string replacement)
            {
                return replacement != null ? Select(_ => replacement) : Select();
            }

            static Func<Match, IEnumerator<int>, string> Replace(
                string replacement, string token)
            {
                return replacement != null && replacement.Contains(token)
                     ? Select(s => s != null ? replacement.ReplaceFirstOccurence(token, s) : replacement)
                     : Replace(replacement);
            }

            static Func<Match, IEnumerator<int>, string> Select()
            {
                return Select(v => v);
            }

            static Func<Match, IEnumerator<int>, T> Select<T>(Func<string, T> selector)
            {
                return (m, num) =>
                {
                    if (!num.MoveNext()) throw new InvalidOperationException();
                    var groups = m.Groups; Group group;
                    return selector(num.Current <= groups.Count && (group = groups[num.Current]).Success
                                    ? group.Value : null);
                };
            }

            static Func<string, T> Create<T>(Regex regex, Func<Match, IEnumerator<int>, T> binder)
            {
                return input =>
                {
                    var m = regex.Match(input);
                    var num = Generate(1, n => n + 1);
                    return m.Success ? binder(m, num) : default(T);
                };
            }

            static IEnumerator<T> Generate<T>(T initial, Func<T, T> next)
            {
                for (var state = initial; ; state = next(state))
                    yield return state;
                // ReSharper disable once FunctionNeverReturns
            }
        }
    }

    static class RegexBinderBuilder
    {
        public static Func<Match, IEnumerator<int>, TResult> SelectMany<T1, T2, TResult>(
            this Func<Match, IEnumerator<int>, T1> binder,
            Func<T1, Func<Match, IEnumerator<int>, T2>> continuation,
            Func<T1, T2, TResult> projection)
        {
            return (m, num) => { T1 f; return projection(f = binder(m, num), continuation(f)(m, num)); };
        }
    }

    static class StringExtensions
    {
        public static string ReplaceFirstOccurence(this string input, string search, string replacement)
        {
            if (input == null) throw new ArgumentNullException("input");
            var index = input.IndexOf(search, StringComparison.Ordinal);
            return index >= 0
                 ? input.Substring(0, index) + replacement + input.Substring(index + search.Length)
                 : input;
        }
    }

    static class DictionaryExtensions
    {
        public static TValue Find<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");
            TValue result;
            return dictionary.TryGetValue(key, out result) ? result : default(TValue);
        }
    }

    /// <summary>
    /// Just enough string parsing to recognize the regexes.yaml file format. Introduced to remove
    /// dependency on large Yaml parsing lib. Note that a unittest ensures compatibility
    /// by ensuring regexes and properties are read similar to using the full yaml lib
    /// </summary>
    internal class MinimalYamlParser
    {
        internal class Mapping
        {
            private Dictionary<string, string> m_lastEntry;

            public Mapping()
            {
                Sequences = new List<Dictionary<string, string>>();
            }

            public List<Dictionary<string, string>> Sequences
            {
                get; private set;
            }

            public void BeginSequence()
            {
                m_lastEntry = new Dictionary<string, string>();
                Sequences.Add(m_lastEntry);
            }

            public void AddToSequence(string key, string value)
            {
                m_lastEntry[key] = value;
            }
        }

        private readonly Dictionary<string, Mapping> m_mappings = new Dictionary<string, Mapping>();

        public MinimalYamlParser(string yamlString)
        {
            ReadIntoMappingModel(yamlString);
        }

        internal IDictionary<string, Mapping> Mappings
        {
            get
            {
                return m_mappings;
            }
        }

        private void ReadIntoMappingModel(string yamlInputString)
        {
            // line splitting using various splitting characters
            string[] lines = yamlInputString.Split(new[] { Environment.NewLine, "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            int lineCount = 0;
            Mapping activeMapping = null;

            foreach (var line in lines)
            {
                lineCount++;
                if (line.Trim().StartsWith("#")) //skipping comments
                    continue;
                if (line.Trim().Length == 0)
                    continue;

                //is this a new mapping entity
                if (line[0] != ' ')
                {
                    int indexOfMappingColon = line.IndexOf(':');
                    if (indexOfMappingColon == -1)
                        throw new ArgumentException("YamlParsing: Expecting mapping entry to contain a ':', at line " + lineCount);
                    string name = line.Substring(0, indexOfMappingColon).Trim();
                    activeMapping = new Mapping();
                    m_mappings.Add(name, activeMapping);
                    continue;
                }

                //reading scalar entries into the active mapping
                if (activeMapping == null)
                    throw new ArgumentException("YamlParsing: Expecting mapping entry to contain a ':', at line " + lineCount);

                var seqLine = line.Trim();
                if (seqLine[0] == '-')
                {
                    activeMapping.BeginSequence();
                    seqLine = seqLine.Substring(1);
                }

                int indexOfColon = seqLine.IndexOf(':');
                if (indexOfColon == -1)
                    throw new ArgumentException("YamlParsing: Expecting scalar mapping entry to contain a ':', at line " + lineCount);

                string key = seqLine.Substring(0, indexOfColon).Trim();
                string value = ReadQuotedValue(seqLine.Substring(indexOfColon + 1).Trim());
                activeMapping.AddToSequence(key, value);
            }
        }

        private static string ReadQuotedValue(string value)
        {
            if (value.StartsWith("'") && value.EndsWith("'"))
                return value.Substring(1, value.Length - 2);
            if (value.StartsWith("\"") && value.EndsWith("\""))
                return value.Substring(1, value.Length - 2);
            return value;
        }

        public IEnumerable<Dictionary<string, string>> ReadMapping(string mappingName)
        {
            Mapping mapping;
            if (m_mappings.TryGetValue(mappingName, out mapping))
            {
                foreach (var s in mapping.Sequences)
                {
                    var temp = s;
                    yield return temp;
                }
            }
        }
    }

}