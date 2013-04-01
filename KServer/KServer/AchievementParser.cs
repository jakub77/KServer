using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace KServer
{
    public static class AchievementParser
    {
        private static string ClauseKeywordToString(AchievementSelect a)
        {
            string keyword = string.Empty;
            Response r = new Response();
            switch (a.clauseKeyword)
            {
                case ClauseKeyword.Artist:
                    keyword = "Artist";
                    break;
                case ClauseKeyword.Title:
                    keyword = "Title";
                    break;
                case ClauseKeyword.SongID:
                    keyword = "SongID";
                    break;
                default:
                    return string.Empty;
            }
            return keyword;
        }

        private static string SelectKeywordToString(AchievementSelect a)
        {
            string keyword = string.Empty;

            switch (a.selectKeyword)
            {
                case SelectKeyword.Max:
                case SelectKeyword.Newest:
                    keyword = "desc";
                    break;
                case SelectKeyword.Min:
                case SelectKeyword.Oldest:
                    keyword = "asc";
                    break;
                case SelectKeyword.CountEqual:
                    keyword = "=";
                    break;
                case SelectKeyword.CountNotEqual:
                    keyword = "!=";
                    break;
                case SelectKeyword.CountGreaterThan:
                    keyword = ">";
                    break;
                case SelectKeyword.CountLessThan:
                    keyword = "<";
                    break;
                default:
                    return string.Empty;
            }
            return keyword;
        }

        private static Response CreateStatementCount(AchievementSelect a, int DJID, out SqlCommand cmd)
        {
            Response r = new Response();
            cmd = new SqlCommand();
            int value;
            if (!int.TryParse(a.selectValue, out value))
            {
                r.error = true;
                r.message = "Could not parse offset";
                return r;
            }

            cmd.CommandText = "select MobileID from MobileSongHistory inner join DJSongs on MobileSongHistory.SongID = DJSongs.SongID ";
            cmd.CommandText += "where DJSongs." + ClauseKeywordToString(a) + " like @clauseKeyword ";
            cmd.Parameters.AddWithValue("@clauseKeyword", a.clauseValue);
            cmd.CommandText += "and VenueID = @DJID and DateSung > @minDate and DateSung < @maxDate ";
            cmd.Parameters.AddWithValue("@DJID", DJID);
            cmd.Parameters.AddWithValue("@minDate", a.startDate);
            cmd.Parameters.AddWithValue("@maxDate", a.endDate);
            cmd.CommandText += "group by MobileID having count(mobileID) " + SelectKeywordToString(a) + " @value;";
            cmd.Parameters.AddWithValue("@value", a.selectValue);
            return r;
        }

        private static Response CreateStatementMinMax(AchievementSelect a, int DJID, out SqlCommand cmd)
        {
            // select MobileID from MobileSongHistory where Artist = 'beaTles' and VenueID = '4' and DateSung > '2000' and DateSung < '2024' 
            // group by MobileID order by count(MobileID) desc offset 0 rows fetch next 1 rows only;
            Response r = new Response();
            cmd = new SqlCommand();
            int offset;
            if(!int.TryParse(a.selectValue, out offset))
            {
                r.error = true;
                r.message="Could not parse offset";
                return r;
            }
            offset--;

            cmd.CommandText = "select MobileID from MobileSongHistory inner join DJSongs on MobileSongHistory.SongID = DJSongs.SongID ";      
            cmd.CommandText+= "where DJSongs." + ClauseKeywordToString(a) + " like @clauseKeyword ";
            cmd.Parameters.AddWithValue("@clauseKeyword", a.clauseValue);
            cmd.CommandText += "and VenueID = @DJID and DateSung > @minDate and DateSung < @maxDate ";
            cmd.Parameters.AddWithValue("@DJID", DJID);
            cmd.Parameters.AddWithValue("@minDate", a.startDate);
            cmd.Parameters.AddWithValue("@maxDate", a.endDate);
            cmd.CommandText += "group by MobileID order by count(MobileID) " + SelectKeywordToString(a) + " ";
            cmd.CommandText += "offset @offset rows fetch next @count rows only;";
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@count", 1);
            return r;
        }

        private static Response CreateStatementOldestNewest(AchievementSelect a, int DJID, out SqlCommand cmd)
        {
            // select MobileID from MobileSongHistory where Artist = 'beatles' and VenueID = '4' and DateSung > '2000' and DateSung < '2020'
            // order by DateSung asc offset 0 rows fetch next 1 row only;
            Response r = new Response();
            cmd = new SqlCommand();
            int offset;
            if (!int.TryParse(a.selectValue, out offset))
            {
                r.error = true;
                r.message = "Could not parse offset";
                return r;
            }
            offset--;

            cmd.CommandText = "select MobileID from MobileSongHistory inner join DJSongs on MobileSongHistory.SongID = DJSongs.SongID ";      
            cmd.CommandText+= "where DJSongs." + ClauseKeywordToString(a) + " like @clauseKeyword ";
            cmd.Parameters.AddWithValue("@clauseKeyword", a.clauseValue);
            cmd.CommandText += "and VenueID = @DJID and DateSung > @minDate and DateSung < @maxDate ";
            cmd.Parameters.AddWithValue("@DJID", DJID);
            cmd.Parameters.AddWithValue("@minDate", a.startDate);
            cmd.Parameters.AddWithValue("@maxDate", a.endDate);
            cmd.CommandText += "order by DateSung " + SelectKeywordToString(a) + " ";
            cmd.CommandText += "offset @offset rows fetch next @count rows only;";
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@count", 1);
            return r;
        }

        private static Response CreateStatementGeneric(AchievementSelect a, int DJID, out SqlCommand cmd)
        {
            switch (a.selectKeyword)
            {
                case SelectKeyword.Max:
                case SelectKeyword.Min:
                    return CreateStatementMinMax(a, DJID, out cmd);
                case SelectKeyword.Newest:    
                case SelectKeyword.Oldest:
                    return CreateStatementOldestNewest(a, DJID, out cmd);
                case SelectKeyword.CountEqual:
                case SelectKeyword.CountNotEqual:
                case SelectKeyword.CountGreaterThan:
                case SelectKeyword.CountLessThan:
                    return CreateStatementCount(a, DJID, out cmd);
                default:
                    Response r = new Response();
                    r.error = true;
                    r.message = "Bad select keyword CreateStatementGeneric";
                    cmd = new SqlCommand();
                    return r;
            }
        }

        private static string CreateDebugSQLText(SqlCommand cmd)
        {
            string query = cmd.CommandText;
            foreach (SqlParameter p in cmd.Parameters)
                query = query.Replace(p.ParameterName, p.Value.ToString());
            return query;
        }

        public static Response CreateAchievementSQL(Achievement a, int DJID, out string sql, out List<SqlCommand> sqlCommands)
        {
            List<SqlCommand> commands = new List<SqlCommand>();
            Response r = new Response();
            SqlCommand cmd;
            sql = string.Empty;
            sqlCommands = new List<SqlCommand>();
            foreach (AchievementSelect achieveSel in a.selectList)
            {
                r = CreateStatementGeneric(achieveSel, DJID, out cmd);
                if (r.error)
                    return r;
                sqlCommands.Add(cmd);
                sql += CreateDebugSQLText(cmd) + "\n";             
            }


            return r;
        }
    }
}