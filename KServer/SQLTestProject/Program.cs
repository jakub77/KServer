using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SQLTestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            //SqlConnection con = new SqlConnection("user id=jakub;pwd=wcoui4;server=localhost\\SQLEXPRESS;Trusted_Connection=yes;database=TestDB2;connection timeout=5");
            SqlConnection con = new SqlConnection("user id=karaoke;pwd=topsecret;server=sunny.eng.utah.edu;database=TestDB2;connection timeout=5");
            try
            {
                con.Open();
                //var products = from Table_1 in 
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                Console.ReadKey();
                return;
            }

            for (; ; )
            {
                try
                {
                    Console.WriteLine("Commands: insert<i>, delete<d>, list<l>, quit<q>");
                    string reply = Console.ReadLine();
                    if (reply.ToLower().StartsWith("i"))
                    {
                        Console.WriteLine("Inserting: <Name, Color, Quantity>");
                        string line = Console.ReadLine();
                        string[] lines = line.Split(',');
                        string command = "insert into Table_2 (Name, Color, Quantity) Values (";
                        command += "'" + lines[0].Trim() + "',";
                        command += "'" + lines[1].Trim() + "',";
                        command += "'" + lines[2].Trim() + "'";
                        command += ");";
                        SqlCommand c = new SqlCommand(command, con);
                        c.ExecuteNonQuery();
                        //Console.WriteLine(command);
                    }
                    else if (reply.ToLower().StartsWith("d"))
                    {
                        Console.WriteLine("Delete <Name>");
                        string name = Console.ReadLine();
                        string command = "delete from Table_2 where Name = ";
                        command += "'" + name + "';";
                        //Console.WriteLine(command);
                        SqlCommand c = new SqlCommand(command, con);
                        c.ExecuteNonQuery();
                    }
                    else if (reply.ToLower().StartsWith("l"))
                    {
                        SqlDataReader r = null;
                        SqlCommand c = new SqlCommand("select * from Table_2", con);
                        r = c.ExecuteReader();
                        while (r.Read())
                        {
                            Console.WriteLine(r[0].ToString() + " " + r[1].ToString() + " " + r[2].ToString());
                        }
                        r.Close();
                    }
                    else if (reply.ToLower().StartsWith("q"))
                        break;
                    else
                        Console.WriteLine("Invalid command");

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }
            }
            Console.WriteLine("Done!");
            Console.ReadKey();
        }
    }
}
