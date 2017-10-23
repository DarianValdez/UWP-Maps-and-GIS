using Microsoft.Data.Sqlite;
using Microsoft.Data.Sqlite.Internal;
using System;
using Windows.Storage;

namespace OfflineMaps.Models
{
    class DatabaseModel : IDisposable
    {
        private StorageFile _databaseFile;
        /// <summary>
        /// SQLite Database file on the filesystem.
        /// </summary>
        public StorageFile DatabaseFile
        {
            get { return _databaseFile; }
            set
            {
                _databaseFile = value;
                if (_databaseFile != null)
                    CreateConnection(_databaseFile.Path);
            }
        }

        /// <summary>
        /// Connection to database.
        /// </summary>
        public SqliteConnection Connection { get; set; }

        #region SQLite Commands

        //private static string _createDataTableString = "CREATE TABLE Route (MeterId TEXT UNIQUE, Lat REAL, Lon REAL)";
        //private SqliteCommand _createDataTableCommand = new SqliteCommand(_createDataTableString);
        ///// <summary>
        ///// Command for creating a Meters table.
        ///// </summary>
        //public SqliteCommand CreateDataTableCommand
        //{
        //    get { return _createDataTableCommand; }
        //    set { _createDataTableCommand = value; }
        //}

        //private static string _insertMeterRowString = "INSERT INTO Meters VALUES (@MeterId,@Lat,@Lon)";
        //private SqliteCommand _insertMeterRowCommand = new SqliteCommand(_insertMeterRowString);
        ///// <summary>
        ///// Command for Inserting a row into the Meters table. 17 Parameters.
        ///// </summary>
        //public SqliteCommand InsertMeterRowCommand
        //{
        //    get { return _insertMeterRowCommand; }
        //    set { _insertMeterRowCommand = value; }
        //}

        private static string _selectMetersString = "SELECT * FROM Route";
        private SqliteCommand _selectMetersCommand = new SqliteCommand(_selectMetersString);
        /// <summary>
        /// Command for Selecting all rows from the Meters table.
        /// </summary>
        public SqliteCommand SelectMetersCommand
        {
            get { return _selectMetersCommand; }
            set { _selectMetersCommand = value; }
        }

        private static string _updateReadString = "UPDATE Route " +
                                                  "SET PreviousReading = (SELECT MeterReading FROM Readings WHERE MeterId = @Id) " +
                                                  "WHERE MeterId = @Id; " +
                                                  "UPDATE Readings SET MeterReading = @Read, ReadingDate = @Date, ReadingTime = @Time, Codes = @Codes, RSSI = @RSSI WHERE MeterId = @Id; " +
                                                  "INSERT OR IGNORE INTO Readings (MeterID, MeterReading, ReadingDate, ReadingTime, Codes, RSSI) " +
                                                  "VALUES (@Id, @Read, @Date, @Time, @Codes, @RSSI);";
        private SqliteCommand _updateReadCommand = new SqliteCommand(_updateReadString);
        /// <summary>
        /// One-stop command for recording a read. Updates 'previous' fields in ROUTE, and UPSERTs in READINGS table.
        /// </summary>
        public SqliteCommand UpdateReadCommand
        {
            get { return _updateReadCommand; }
            set { _updateReadCommand = value; }
        }

        #endregion

        public DatabaseModel()
        {
            //Forces the use of Windows 10 packaged SQLite.
            SqliteEngine.UseWinSqlite3();
        }

        /// <summary>
        /// Creates a SQLite connection and initializes all commands with it. Creates database file if necessary.
        /// </summary>
        /// <param name="FilePath">Path of database to be connected to.</param>
        /// <returns></returns>
        private void CreateConnection(string FilePath)
        {
            try
            {
                //Build the connection string
                SqliteConnectionStringBuilder connBuilder = new SqliteConnectionStringBuilder()
                {
                    Mode = SqliteOpenMode.ReadWrite,
                    Cache = SqliteCacheMode.Default,
                    DataSource = FilePath
                };
                Connection = new SqliteConnection(connBuilder.ToString());

                //Init Commands
                //CreateDataTableCommand.Connection = Connection;
                //InsertMeterRowCommand.Connection = Connection;
                SelectMetersCommand.Connection = Connection;
                UpdateReadCommand.Connection = Connection;

                //Try to create Meters table. Will throw an exception if table exists.
                //ExecuteCommand(CreateDataTableCommand);
            }
            catch (Exception sqlex)
            {
                //Do nothing
            }
        }

        /// <summary>
        /// Executes a query command. Not suitable for Read commands.
        /// </summary>
        /// <param name="Command">Command to be executed.</param>
        /// <returns></returns>
        public string ExecuteCommand(SqliteCommand Command)
        {
            string ret = "";
            if (Connection != null)
            {
                try
                {
                    //Open, execute, and close
                    
                    Connection.Open();
                    Command.ExecuteNonQuery();
                    Connection.Close();
                }
                catch (Exception sqlex)
                {
                    ret = sqlex.Message;
                }
            }
            //Clear all parameters if they havent already
            Command.Parameters.Clear();
            return ret;
        }

        public void Dispose()
        {
            _selectMetersCommand.Dispose();
            _updateReadCommand.Dispose();
        }
    }
}
