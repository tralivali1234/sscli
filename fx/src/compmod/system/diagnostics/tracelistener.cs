//------------------------------------------------------------------------------
// <copyright file="TraceListener.cs" company="Microsoft">
//     
//      Copyright (c) 2006 Microsoft Corporation.  All rights reserved.
//     
//      The use and distribution terms for this software are contained in the file
//      named license.txt, which can be found in the root of this distribution.
//      By using this software in any fashion, you are agreeing to be bound by the
//      terms of this license.
//     
//      You must not remove this notice, or any other, from this software.
//     
// </copyright>
//------------------------------------------------------------------------------

/*
 */
namespace System.Diagnostics {
    using System;
    using System.Text;
    using System.Security.Permissions;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Collections;
    using System.Configuration;
    using System.Runtime.Versioning;
    
    /// <devdoc>
    /// <para>Provides the <see langword='abstract '/>base class for the listeners who
    ///    monitor trace and debug output.</para>
    /// </devdoc>
    [HostProtection(Synchronization=true)]
    public abstract class TraceListener : MarshalByRefObject, IDisposable {

        int indentLevel;
        int indentSize = 4;
        TraceOptions traceOptions = TraceOptions.None;
        bool needIndent = true;

        string listenerName;
        TraceFilter filter = null;
        StringDictionary attributes;
        internal string initializeData;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.TraceListener'/> class.</para>
        /// </devdoc>
        protected TraceListener () {
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.TraceListener'/> class using the specified name as the
        ///    listener.</para>
        /// </devdoc>
        protected TraceListener(string name) {
            this.listenerName = name;
        }

        public StringDictionary Attributes {
            get {
                if (attributes == null)
                    attributes = new StringDictionary();
                return attributes;
            }
        }

        /// <devdoc>
        /// <para> Gets or sets a name for this <see cref='System.Diagnostics.TraceListener'/>.</para>
        /// </devdoc>
        public virtual string Name {
            get { return (listenerName == null) ? "" : listenerName; }

            set { listenerName = value; }
        }

        public virtual bool IsThreadSafe {
            get { return false; }
        }

        /// <devdoc>
        /// </devdoc>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <devdoc>
        /// </devdoc>
        protected virtual void Dispose(bool disposing) {
            return;
        }


        /// <devdoc>
        ///    <para>When overridden in a derived class, closes the output stream
        ///       so that it no longer receives tracing or debugging output.</para>
        /// </devdoc>
        public virtual void Close() {
            return;
        }

        /// <devdoc>
        ///    <para>When overridden in a derived class, flushes the output buffer.</para>
        /// </devdoc>
        public virtual void Flush() {
            return;
        }

        /// <devdoc>
        ///    <para>Gets or sets the indent level.</para>
        /// </devdoc>
        public int IndentLevel {
            get {
                return indentLevel;
            }

            set {
                indentLevel = (value < 0) ? 0 : value;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the number of spaces in an indent.</para>
        /// </devdoc>
        public int IndentSize {
            get {
                return indentSize;
            }

            set {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("IndentSize", value, SR.GetString(SR.TraceListenerIndentSize));
                indentSize = value;
            }
        }

        [
        ComVisible(false)
        ]
        public TraceFilter Filter {
            get { 
                return filter; 
            }
            set {
                filter = value;
            }
        }

        
        /// <devdoc>
        ///    <para>Gets or sets a value indicating whether an indent is needed.</para>
        /// </devdoc>
        protected bool NeedIndent {
            get {
                return needIndent;
            }

            set {
                needIndent = value;
            }
        }

        [
        ComVisible(false)
        ]
        public TraceOptions TraceOutputOptions {
            get { return traceOptions; }
            set {
                if (( (int) value >> 6) != 0) {
                    throw new ArgumentOutOfRangeException("value");
                }

                traceOptions = value;
            }
        }

        internal void SetAttributes(Hashtable attribs) {
            TraceUtils.VerifyAttributes(attribs, GetSupportedAttributes(), this);

            attributes = new StringDictionary();
            attributes.contents = attribs;
        }
        
        /// <devdoc>
        ///    <para>Emits or displays a message for an assertion that always fails.</para>
        /// </devdoc>
        public virtual void Fail(string message) {
            Fail(message, null);
        }

        /// <devdoc>
        ///    <para>Emits or displays messages for an assertion that always fails.</para>
        /// </devdoc>
        public virtual void Fail(string message, string detailMessage) {
            StringBuilder failMessage = new StringBuilder();
            failMessage.Append(SR.GetString(SR.TraceListenerFail));
            failMessage.Append(" ");
            failMessage.Append(message);
            if (detailMessage != null) {
                failMessage.Append(" ");
                failMessage.Append(detailMessage);
            }

            WriteLine(failMessage.ToString());
        }

        virtual protected internal string[] GetSupportedAttributes() {
            return null;
        }

        /// <devdoc>
        ///    <para>When overridden in a derived class, writes the specified
        ///       message to the listener you specify in the derived class.</para>
        /// </devdoc>
        public abstract void Write(string message);

        /// <devdoc>
        /// <para>Writes the name of the <paramref name="o"/> parameter to the listener you specify when you inherit from the <see cref='System.Diagnostics.TraceListener'/>
        /// class.</para>
        /// </devdoc>
        public virtual void Write(object o) {
            if (Filter != null && !Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, null, null, o)) 
                return;

            if (o == null) return;
            Write(o.ToString());
        }

        /// <devdoc>
        ///    <para>Writes a category name and a message to the listener you specify when you
        ///       inherit from the <see cref='System.Diagnostics.TraceListener'/>
        ///       class.</para>
        /// </devdoc>
        public virtual void Write(string message, string category) {
            if (Filter != null && !Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, message)) 
                return;

            if (category == null)
                Write(message);
            else
                Write(category + ": " + ((message == null) ? string.Empty : message));
        }

        /// <devdoc>
        /// <para>Writes a category name and the name of the <paramref name="o"/> parameter to the listener you
        ///    specify when you inherit from the <see cref='System.Diagnostics.TraceListener'/>
        ///    class.</para>
        /// </devdoc>
        public virtual void Write(object o, string category) {
            if (Filter != null && !Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, category, null, o)) 
                return;

            if (category == null)
                Write(o);
            else
                Write(o == null ? "" : o.ToString(), category);
        }

        /// <devdoc>
        ///    <para>Writes the indent to the listener you specify when you
        ///       inherit from the <see cref='System.Diagnostics.TraceListener'/>
        ///       class, and resets the <see cref='TraceListener.NeedIndent'/> property to <see langword='false'/>.</para>
        /// </devdoc>
        protected virtual void WriteIndent() {
            NeedIndent = false;
            for (int i = 0; i < indentLevel; i++) {
                if (indentSize == 4)
                    Write("    ");
                else {
                    for (int j = 0; j < indentSize; j++) {
                        Write(" ");
                    }
                }
           }
        }

        /// <devdoc>
        ///    <para>When overridden in a derived class, writes a message to the listener you specify in
        ///       the derived class, followed by a line terminator. The default line terminator is a carriage return followed
        ///       by a line feed (\r\n).</para>
        /// </devdoc>
        public abstract void WriteLine(string message);

        /// <devdoc>
        /// <para>Writes the name of the <paramref name="o"/> parameter to the listener you specify when you inherit from the <see cref='System.Diagnostics.TraceListener'/> class, followed by a line terminator. The default line terminator is a
        ///    carriage return followed by a line feed
        ///    (\r\n).</para>
        /// </devdoc>
        public virtual void WriteLine(object o) {
            if (Filter != null && !Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, null, null, o)) 
                return;

            WriteLine(o == null ? "" : o.ToString());
        }

        /// <devdoc>
        ///    <para>Writes a category name and a message to the listener you specify when you
        ///       inherit from the <see cref='System.Diagnostics.TraceListener'/> class,
        ///       followed by a line terminator. The default line terminator is a carriage return followed by a line feed (\r\n).</para>
        /// </devdoc>
        public virtual void WriteLine(string message, string category) {
            if (Filter != null && !Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, message)) 
                return;
            if (category == null)
                WriteLine(message);
            else
                WriteLine(category + ": " + ((message == null) ? string.Empty : message));
        }

        /// <devdoc>
        ///    <para>Writes a category
        ///       name and the name of the <paramref name="o"/>parameter to the listener you
        ///       specify when you inherit from the <see cref='System.Diagnostics.TraceListener'/>
        ///       class, followed by a line terminator. The default line terminator is a carriage
        ///       return followed by a line feed (\r\n).</para>
        /// </devdoc>
        public virtual void WriteLine(object o, string category) {
            if (Filter != null && !Filter.ShouldTrace(null, "", TraceEventType.Verbose, 0, category, null, o)) 
                return;

            WriteLine(o == null ? "" : o.ToString(), category);
        }


        // new write methods used by TraceSource

        [
        ComVisible(false)
        ]
    	public virtual void TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, int id, object data) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data)) 
                return;

            WriteHeader(source, eventType, id);
            string datastring = String.Empty;
            if (data != null)
                datastring = data.ToString();

            WriteLine(datastring);
            WriteFooter(eventCache);
    	}

        [
        ComVisible(false)
        ]
    	public virtual void TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, int id, params object[] data) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data)) 
                return;

            WriteHeader(source, eventType, id);

            StringBuilder sb = new StringBuilder();
            if (data != null) {
                for (int i=0; i< data.Length; i++) {
                    if (i != 0)
                        sb.Append(", ");

                    if (data[i] != null)
                        sb.Append(data[i].ToString());
                }
            }
            WriteLine(sb.ToString());
            
            WriteFooter(eventCache);
    	}

        [
        ComVisible(false)
        ]
    	public virtual void TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, int id) {
    	    TraceEvent(eventCache, source, eventType, id, String.Empty);
    	}

        // All other TraceEvent methods come through this one.
        [
        ComVisible(false)
        ]
        public virtual void TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, int id, string message) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, message)) 
                return;

            WriteHeader(source, eventType, id);
            WriteLine(message);

            WriteFooter(eventCache);
        }

        [
        ComVisible(false)
        ]
        public virtual void TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, int id, string format, params object[] args) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, format, args)) 
                return;

            WriteHeader(source, eventType, id);
            if (args != null)
                WriteLine(String.Format(CultureInfo.InvariantCulture, format, args));
            else
                WriteLine(format);

            WriteFooter(eventCache);
        }

        [
        ComVisible(false)
        ]
        public virtual void TraceTransfer(TraceEventCache eventCache, String source, int id, string message, Guid relatedActivityId) {
            TraceEvent(eventCache, source, TraceEventType.Transfer, id, message + ", relatedActivityId=" + relatedActivityId.ToString()); 
        }

        private void WriteHeader(String source, TraceEventType eventType, int id) {
            Write(String.Format(CultureInfo.InvariantCulture, "{0} {1}: {2} : ", source, eventType.ToString(), id.ToString(CultureInfo.InvariantCulture)));
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        private void WriteFooter(TraceEventCache eventCache) {
            if (eventCache == null)
                return;
            
            indentLevel++;
            if (IsEnabled(TraceOptions.ProcessId))
                WriteLine("ProcessId=" + eventCache.ProcessId);

            if (IsEnabled(TraceOptions.LogicalOperationStack)) {
                Write("LogicalOperationStack=");
                Stack operationStack = eventCache.LogicalOperationStack;
                bool first = true;
                foreach (Object obj in operationStack) {
                    if (!first)
                        Write(", ");
                    else
                        first = false;
                    
                    Write(obj.ToString());
                }
                WriteLine(String.Empty);
            }

            if (IsEnabled(TraceOptions.ThreadId))
                WriteLine("ThreadId=" + eventCache.ThreadId);

            if (IsEnabled(TraceOptions.DateTime))
                WriteLine("DateTime=" + eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));

            if (IsEnabled(TraceOptions.Timestamp))
                WriteLine("Timestamp=" + eventCache.Timestamp);

            if (IsEnabled(TraceOptions.Callstack))
                WriteLine("Callstack=" + eventCache.Callstack);
            indentLevel--;
        }

        internal bool IsEnabled(TraceOptions opts) {
            return (opts & TraceOutputOptions) != 0;
        }
    }
}
