using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using ZhFramework.Engine.Utilities;
using Battle.Core;
using System.Text;
using OfficeOpenXml;

namespace Battle.Level.Editor
{


    public partial class LevelEditor : EditorWindow
    {


        public static string kExcelRoot = $"../tables/Excel/Common/";

        #region 服务器TODO

        /// <summary>
        /// 服务器导表函数
        /// </summary>
        private void OnServerClick()
        {
            //创建表格
            var excelPath = CreateProfileExcel("StageEvent", out var excelPackage);
            var files = Directory.GetFiles(kRootPath);
            var configs = new List<LevelConfig>();
            var names = new List<string>();

            
            foreach(var fileName in files)
            {
                if (fileName.EndsWith("meta"))
                {
                    continue;
                }
                var bytes = File.ReadAllBytes(fileName);
                var configData = Encoding.UTF8.GetString(bytes);
                configs.Add(JsonUtils.ToObject<LevelConfig>(configData));
                names.Add(Path.GetFileNameWithoutExtension(fileName));
            }
            var sortDict = new SortedDictionary<int, LevelConfig>();
            var nameDict = new SortedDictionary<int, string>();
            for(int i = 0; i<names.Count;i++)
            {
                var id = Convert.ToInt32(names[i].Split('_')[1]);
                sortDict.Add(id, configs[i]); 
                nameDict.Add(id , names[i]);
            }
            configs = sortDict.Values.ToList<LevelConfig>();
            names = nameDict.Values.ToList<string>();
            //创建表格页，具体结构在函数内修改
            CreateFrameSheet(excelPackage.Workbook, configs, names);
            excelPackage.Save();
        }
        /// <summary>
        ///创建excel表格文件
        /// </summary>
        static string CreateProfileExcel(string path, out ExcelPackage excel)
        {
            string directory =kExcelRoot;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var fileName = $"{directory}/{path}.xlsx";
            var newFile = new FileInfo(fileName);
            if (newFile.Exists)
                newFile.Delete();

            excel = new ExcelPackage(newFile);

            return fileName;
        }
        /// <summary>
        ///创建sheet
        /// </summary>
        static void CreateFrameSheet(ExcelWorkbook workbook, List<LevelConfig> configs, List<string> names)
        {
            //第一张表
            var stageConfig = ExcelUtils.CreateSheet(workbook, "Stage", "cs","","cs","cs");
            
            var rowStageConfig = stageConfig.Dimension.Rows;
            ++rowStageConfig;
            var col = 1;
            stageConfig.Cells[rowStageConfig, col++].Value = "Id";
            stageConfig.Cells[rowStageConfig, col++].Value = "Name";
            stageConfig.Cells[rowStageConfig, col++].Value = "Round";
            stageConfig.Cells[rowStageConfig, col++].Value = "Countdown";
            ++rowStageConfig;
            col = 1;
            stageConfig.Cells[rowStageConfig, col++].Value = "int";
            stageConfig.Cells[rowStageConfig, col++].Value = "string";
            stageConfig.Cells[rowStageConfig, col++].Value = "int";
            stageConfig.Cells[rowStageConfig, col++].Value = "int";
            ++rowStageConfig;
            col = 1;
            stageConfig.Cells[rowStageConfig, col++].Value = "编号";
            stageConfig.Cells[rowStageConfig, col++].Value = "关卡名字";
            stageConfig.Cells[rowStageConfig, col++].Value = "最大回合数";
            stageConfig.Cells[rowStageConfig, col++].Value = "出手倒计时";
            for (int i = 0; i < configs.Count; i++)
            {
                var data = configs[i];
                ++rowStageConfig;
                col = 1;
                var nameId = names[i].Split('_');
                stageConfig.Cells[rowStageConfig, col++].Value = Convert.ToInt32(nameId[1]);
                stageConfig.Cells[rowStageConfig, col++].Value = names[i];
                stageConfig.Cells[rowStageConfig, col++].Value = data.Data.MaxRound;
                stageConfig.Cells[rowStageConfig, col++].Value = data.Data.MaxInputDuration;
            }

            stageConfig.Cells.AutoFitColumns();


            //第二张表
            var sheetStageEventConfig = ExcelUtils.CreateSheet(workbook, "StageEvent", "cs", "cs",  "cs","cs","cs","cs","cs","cs");
            var rowStageEventConfig = sheetStageEventConfig.Dimension.Rows;
            ++rowStageEventConfig;
            col = 1;
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "Id";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "EventType";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "ConditionParam";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "Camp";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "StageId";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "MonsterId";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "Wave";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "Stand";
            ++rowStageEventConfig;

            col = 1;
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "int";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "int";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "double_slc|int";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "int";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "int";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "int";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "int";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "int";
            ++rowStageEventConfig;
            col = 1;
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "流水ID";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "类型";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "触发条件参数";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "阵营";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "关卡id";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "怪物id";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "波次";
            sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = "站位";
            for (int i = 0; i < configs.Count; i++)
            {
                var data = configs[i];


                var nameId = names[i].Split('_');
                var stageId = Convert.ToInt32(nameId[1]);
                int eventCount = 0;
                for (int j = 0; j < data.Process.Modules.Count; j++)
                {
                    var process = data.Process.Modules[j];
                    var uid = process.Uid;
                    switch (process.Id)
                    {
                        case ELevelModuleId.CreateEnemy:

                            foreach (var module in data.Configs.CreateEnemyList)
                            {
                                if (module.Uid == uid)
                                {
                                    var enemies = module.Enemys;
                                    foreach (var enemy in enemies)
                                    {
                                        ++rowStageEventConfig;
                                        col = 1;
                                        sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = stageId * 1000 + eventCount;
                                        sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = enemy.IsSupervise ? 2 : 1;
                                        sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = new List<int>();
                                        sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = enemy.IsAlly ? 1 : 2;
                                        sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = stageId;
                                        sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = enemy.RoleId;
                                        sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = module.wave;
                                        sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = enemy.BornPos;
                                        eventCount++;
                                    }
                                }
                            }
                            break;
                        case ELevelModuleId.AddEnemy:
                            foreach (var module in data.Configs.AddEnemyList)
                            {
                                if (module.Uid == uid)
                                {
                                    LevelConfigHelper.GetConditions(module.ConditionData, out var conditions, out var args);
                                    ++rowStageEventConfig;
                                    col = 1;
                                    sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = stageId * 1000 + eventCount;
                                    sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = 3;
                                    sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = DoubleArrayToString(conditions,args);
                                    sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = module.isAlly ? 1 : 2;
                                    sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = stageId;
                                    sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = module.MonsterId;
                                    sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = module.wave;
                                    sheetStageEventConfig.Cells[rowStageEventConfig, col++].Value = module.BornPosId;
                                    eventCount++;
                                }
                            }
                            break;

                    }
                }


            }


            sheetStageEventConfig.Cells.AutoFitColumns();
        }



        private static string DoubleArrayToString(List<int> conditions, List<List<int>> args)
        {
            string output = "";
            for(int i = 0;i<args.Count;i++)
            {
                var arg = args[i];
                output += conditions[i];
                output += "|";
                foreach (var elemenet in arg)
                {
                    output+= elemenet;
                    output += "|";
                }
                    output = output.Remove(output.Length - 1,1);

                output += ";";
            }
            if (args.Count > 0)
            {
                output = output.Remove(output.Length - 1,1);
            }

            return output;
        }

        #endregion






    }

}