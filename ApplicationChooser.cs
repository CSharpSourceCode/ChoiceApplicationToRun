using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceApplicationToRun
{
    public class ApplicationChooser
    {
        // const string RUNNUMBERKEY_Str = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static void Run(Type type, string[] args)
        {
            Assembly assembly = type.Assembly;
            List<MethodBase> entryMethodPointList = new List<MethodBase>();
            foreach (Type t in assembly.GetTypes())
            {
                if (t == type)
                {
                    continue;
                }
                MethodBase entryMethodPoint = GetMethodEntryPoint(t);
                if (entryMethodPoint != null)
                {
                    entryMethodPointList.Add(entryMethodPoint);
                }
            }
            entryMethodPointList.Sort(delegate(MethodBase method1, MethodBase method2) { return method1.DeclaringType.Name.CompareTo(method2.DeclaringType.Name); });
            int methodListCnt = entryMethodPointList.Count;
            if (methodListCnt == 0)
            {
                Console.WriteLine("没有找到方法的入口点.点击任意键退出");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("-1:表示退出程序");
            List<int> methodIndex = new List<int>(methodListCnt);
            for (int i = 0; i < methodListCnt; i++)
            {
                methodIndex.Add(i);
                Console.WriteLine("{0}:{1}", i, GetEntryMethodPointName(entryMethodPointList[i]));
                //  Console.WriteLine("{0}:{1}", RUNNUMBERKEY_Str[i], GetEntryMethodPointName(entryMethodPointList[i]));
            }
            int entry = -1;
            while (true)
            {
                entry = -1; //RUNNUMBERKEY_Str.IndexOf(input);
                Console.WriteLine();
                Console.Write("请输入运行的代号:");
                Console.Out.Flush();
                string input = Console.ReadLine();
                if (!int.TryParse(input, out entry) || entry >= methodListCnt)
                {
                    Console.WriteLine("无效的输入,请重新输入");
                    continue;
                }
                if (entry == -1)
                {
                    System.Environment.Exit(-1);
                    break;
                }
                try
                {
                    MethodBase main = entryMethodPointList[entry];
                    main.Invoke(null, main.GetParameters().Length == 0 ? null : new object[] { args });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("出现异常:{0}", ex);
                    break;
                }
            }
            Console.ReadLine();
        }
        /// <summary>
        /// 方法的名称
        /// </summary>
        /// <param name="methodBase"></param>
        /// <returns></returns>
        private static object GetEntryMethodPointName(MethodBase methodBase)
        {
            Type type = methodBase.DeclaringType;
            object[] descripts = type.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return descripts.Length == 0 ? type.Name : string.Format("{0}{1}", type.Name, ((DescriptionAttribute)descripts[0]).Description);
        }
        /// <summary>
        /// 返回类中一个方法的入口点，或者如果没有就返回null。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static MethodBase GetMethodEntryPoint(Type type)
        {
            if (type.IsGenericTypeDefinition || type.IsGenericType)
            {
                return null;
            }
            BindingFlags anyStatic = BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo[] methods = type.GetMethods(anyStatic);
            MethodInfo parameterless = null;
            MethodInfo stringArrayParam = null;
            foreach (MethodInfo item in methods)
            {
                if (item.Name != "Main")
                {
                    continue;
                }
                if (item.IsGenericMethod || item.IsGenericMethodDefinition)
                {
                    continue;
                }
                ParameterInfo[] parms = item.GetParameters();
                if (parms.Length == 0)
                {
                    parameterless = item;
                }
                else
                {
                    if (parms.Length == 1 && !parms[0].IsOut && !parms[0].IsOptional && parms[0].ParameterType == typeof(string[]))
                    {
                        stringArrayParam = item;
                    }
                }
            }
            return stringArrayParam ?? parameterless;
        }
    }
}
