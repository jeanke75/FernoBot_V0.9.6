using System;
using Discord;
using Discord.Commands;
using DiscordBot.Helpers;

namespace DiscordBot.Modules.Administration
{
    class SqlHandler
    {
        private readonly DiscordClient client;
        private CommandService service;
        public SqlHandler(DiscordClient client)
        {
            this.client = client;
            service = client.GetService<CommandService>();
            Setup();
        }

        private void Setup()
        {
            service.CreateCommand("sql")
                    .Parameter("SQL", ParameterType.Unparsed)
                    .AddCheck((command, user, channel) => user.Id == 140470317440040960)
                    .Hide()
                    .Do(async (e) =>
                    {
                        if (!e.GetArg("SQL").Trim().Equals(""))
                        {
                            await e.Channel.SendMessage(e.GetArg("SQL").Trim());
                            try
                            {
                                using (System.Data.SqlClient.SqlConnection conn = Helper.getConnection())
                                {
                                    await conn.OpenAsync();
                                    using (System.Data.SqlClient.SqlTransaction tr = conn.BeginTransaction())
                                    {
                                        using (System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand())
                                        {
                                            cmd.Transaction = tr;

                                            cmd.CommandText = e.GetArg("SQL").Trim();
                                            await cmd.ExecuteNonQueryAsync();
                                        }
                                        tr.Commit();
                                    }
                                }
                                await e.Channel.SendMessage("success");
                            }
                            catch (Exception ex)
                            {
                                await e.Channel.SendMessage(ex.ToString());
                            }
                        }
                    });

            service.CreateCommand("tables")
                .Hide()
                .AddCheck((command, user, channel) => user.Id == 140470317440040960)
                .Do(async (e) =>
                {
                    try
                    {
                        using (System.Data.SqlClient.SqlConnection conn = Helper.getConnection())
                        {
                            await conn.OpenAsync();
                            try
                            {
                                System.Data.DataTable schemaDataTable = conn.GetSchema("Tables");
                                string colums = "";
                                foreach (System.Data.DataColumn column in schemaDataTable.Columns)
                                {
                                    colums += column.ColumnName + "\t";
                                }
                                await e.Channel.SendMessage(colums);
                                foreach (System.Data.DataRow row in schemaDataTable.Rows)
                                {
                                    string rows = "";
                                    foreach (object value in row.ItemArray)
                                    {
                                        rows += value.ToString() + "\t";
                                    }
                                    await e.Channel.SendMessage(rows);
                                }
                                await e.Channel.SendMessage("-----done-----");
                            }
                            finally
                            {
                                conn.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage(ex.ToString());
                    }
                });

            service.CreateCommand("select")
                .Hide()
                .Parameter("SQL", ParameterType.Unparsed)
                .AddCheck((command, user, channel) => user.Id == 140470317440040960)
                .Do(async (e) =>
                {
                    if (!e.GetArg("SQL").Trim().Equals(""))
                    {
                        await e.Channel.SendMessage(e.GetArg("SQL").Trim());
                        try
                        {
                            using (System.Data.SqlClient.SqlConnection conn = Helper.getConnection())
                            {
                                await conn.OpenAsync();
                                try
                                {
                                    using (System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = e.GetArg("SQL").Trim();
                                        using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                                        {
                                            while (await reader.ReadAsync())
                                            {
                                                string row = "";
                                                for (int i = 0; i < reader.FieldCount; i++)
                                                {
                                                    row += reader.GetValue(i) + " ";
                                                }

                                                await e.Channel.SendMessage(row);
                                            }
                                        }
                                    }
                                    await e.Channel.SendMessage("-----done-----");
                                }
                                finally
                                {
                                    conn.Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage(ex.ToString());
                        }
                    }
                });
        }
    }
}
