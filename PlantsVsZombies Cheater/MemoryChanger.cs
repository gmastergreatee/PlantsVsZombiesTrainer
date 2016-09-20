using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RajarshiSoftwares
{
    #region MemoryChanger - Version 1.1
    /// <summary>
    /// Lists and controls all the added cheats
    /// Developed by GameMaster Greatee aka Rajarshi Vaidya
    /// </summary>
    public class MemoryChanger
    {
        public delegate void Err(string error);
        /// <summary>
        /// Triggered when an error has occurred either while cheat activation or deactivation
        /// </summary>
        public Err Error;

        /// <summary>
        /// Gets a collection of all added cheats
        /// </summary>
        public Dictionary<int, Cheat> Cheats { get; private set; }
        int i = 0;

        /// <summary>
        /// Returns the number of cheats in the Cheat Collection
        /// </summary>
        public int CheatCount
        {
            get
            {
                return Cheats.Count;
            }
        }

        public MemoryChanger()
        {
            Cheats = new Dictionary<int, Cheat>();
        }

        /// <summary>
        /// Adds a cheat to the collection
        /// </summary>
        /// <param name="newCheat">The object reference to the new Cheat</param>
        /// <returns>True if added, else false</returns>
        public bool AddCheat(Cheat newCheat)
        {
            try
            {
                (newCheat as Errors).errorFound += ErrorFound;
                Cheats.Add(++i, newCheat);
                (newCheat as Errors).Id = i;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes a particular cheat from the collection
        /// </summary>
        /// <param name="cheatID">The ID of the cheat</param>
        /// <returns>True if deleted, else false</returns>
        public bool DeleteCheat(int cheatID)
        {
            if (Cheats.ContainsKey(cheatID))
            {
                Cheats.Remove(cheatID);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Starts a particular cheat
        /// </summary>
        /// <param name="cheatID">The ID of the cheat to start</param>
        /// <returns>True if successful, else false</returns>
        public bool StartCheat(int cheatID)
        {
            if (Cheats.ContainsKey(cheatID))
            {
                Cheats[cheatID].Start();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Stops a particular cheat
        /// </summary>
        /// <param name="cheatID">The ID of the cheat to stop</param>
        /// <returns>True if successful, else false</returns>
        public bool StopCheat(int cheatID)
        {
            if (Cheats.ContainsKey(cheatID))
            {
                Cheats[cheatID].Stop();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a particular cheat is active or not
        /// </summary>
        /// <param name="cheatID">The ID of the cheat</param>
        /// <returns>True if activated, else false</returns>
        public bool CheatActivated(int cheatID)
        {
            if(Cheats.ContainsKey(cheatID))
            {
                return (Cheats[cheatID] as Errors).act;
            }
            return false;
        }

        /// <summary>
        /// Gets the cheat name
        /// </summary>
        /// <param name="cheatID">The ID of the cheat</param>
        /// <returns>The name of the cheat if exists, else ""</returns>
        public string CheatName(int cheatID)
        {
            if (Cheats.ContainsKey(cheatID))
            {
                return (Cheats[cheatID] as Errors).CheatName;
            }
            return "";
        }

        private void ErrorFound(Errors errorDetails)
        {
            Error?.Invoke("Error in " + (string.IsNullOrWhiteSpace(errorDetails.CheatName) ? errorDetails.CheatName : "Cheat ID:" + errorDetails.Id.ToString()));
        }
    }

    public abstract class Errors
    {
        /// <summary>
        /// The name of the cheat - useful in case of error checking
        /// </summary>
        public string CheatName { get; set; }
        /// <summary>
        /// Unique identifier for the cheatName to be used internally by the MemoryChanger class in absense of CheatName
        /// </summary>
        public int Id = 0;
        public bool act { get; set; }
        public delegate void Error(Errors errorDetails);
        public Error errorFound;
    }

    public class ByteNopCheat : Errors, Cheat
    {
        int nopValue = 0x90;
        int bytes = 0;
        Memory angel;
        /// <summary>
        /// The address to start noping from
        /// </summary>
        public long Address { get; set; }
        byte[] stored;

        /// <summary>
        /// Default constructor for ByteNopCheat
        /// </summary>
        /// <param name="Address">The address starting from which to put nop</param>
        /// <param name="memory">The initialized <see cref="Memory"/> object</param>
        public ByteNopCheat(long Address, Memory memory)
            : this(Address, memory, 1)
        { }

        /// <summary>
        /// Default constructor for ByteNopCheat
        /// </summary>
        /// <param name="Address">The address starting from which to put nop</param>
        /// <param name="memory">The initialized <see cref="Memory"/> object</param>
        /// <param name="cheatName">The name of the cheat</param>
        public ByteNopCheat(long Address, Memory memory, string cheatName)
            : this(Address, memory, 1, cheatName)
        { }

        /// <summary>
        /// Default constructor for ByteNopCheat
        /// </summary>
        /// <param name="Address">The address starting from which to put nop</param>
        /// <param name="memory">The initialized <see cref="Memory"/> object</param>
        /// <param name="bytes">The number of bytes to nop</param>
        public ByteNopCheat(long Address, Memory memory, int bytes)
        {
            this.Address = Address;
            this.bytes = bytes;
            CheatName = "";
            angel = memory;
        }

        /// <summary>
        /// Default constructor for ByteNopCheat
        /// </summary>
        /// <param name="Address">The address starting from which to put nop</param>
        /// <param name="memory">The initialized <see cref="Memory"/> object</param>
        /// <param name="bytes">The number of bytes to nop</param>
        /// <param name="cheatName">The name of the cheat</param>
        public ByteNopCheat(long Address, Memory memory, int bytes, string cheatName)
            : this(Address, memory, bytes)
        {
            CheatName = cheatName;
        }

        /// <summary>
        /// Toggle the cheat on
        /// </summary>
        public void Start()
        {
            try
            {
                stored = angel.Read(Address, bytes);
                angel.noperTill(Address, bytes);
                act = true;
            }
            catch
            {
                errorFound?.Invoke(this);
            }
        }

        /// <summary>
        /// Toggle the cheat off
        /// </summary>
        public void Stop()
        {
            if (stored.Length > 0)
            {
                angel.Write(Address, stored);
                act = false;
            }
            else
            {
                errorFound?.Invoke(this);
            }
        }
    }

    public interface Cheat
    {
        void Start();
        void Stop();
    }
    #endregion
}
