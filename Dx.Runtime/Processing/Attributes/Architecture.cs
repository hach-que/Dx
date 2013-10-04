namespace Dx.Runtime
{
    public enum Architecture
    {
        /// <summary>
        /// Specifies that the program should execute in peer-to-peer mode.  In this mode, all programs are
        /// equal and execute their entry points regardless of whether they are the first node in the network.
        /// </summary>
        PeerToPeer,

        /// <summary>
        /// Specifies that the first node in the network should act a server managing all data.  Further connections
        /// to the network will act as slaves, except that the can only call methods with the ClientCallable
        /// attribute applied.  This mode is similar to MasterSlave except that the instruction roles are reversed;
        /// rather than the server telling clients what to do (MasterSlave), clients tell the server what they want
        /// to do (ServerClient).
        /// </summary>
        ServerClient,
    }
}

