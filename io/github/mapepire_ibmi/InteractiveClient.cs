using System;
using io.github.mapepire_ibmi.types;

namespace io.github.mapepire_ibmi { 

public class InteractiveClient {

	private static DaemonServer? daemonServer;
    private static SqlJob? job; 
	public static void Main(string[] args) {

		Console.WriteLine("Running interactive MapepireClient");
		try {
			
			string? line = Console.ReadLine();
			bool running = true;
			while (line != null && running) {
				running = ProcessLine(line);
				line = Console.ReadLine();
			}

		} catch (Exception e) {
			Console.WriteLine("Severe failure");
			Console.WriteLine(e.StackTrace);
		}
	}

	private static bool ProcessLine(string line) {

		try { 
		string[] lineElements = splitLine(line);
		if (lineElements.Length > 0) {
            string command = lineElements[0].ToLowerInvariant(); 
			if ("connect".Equals(command)) {
				connect(lineElements);
			} else if ("runquery".Equals(command)) {
				RunQuery(line);
			} else if ("help".Equals(command)) {
				ProcessHelp(); 
			} else if ("exit".Equals(command)) {
				return false;
			} else {
				Console.WriteLine("Did not recognize command: " + line);
				ProcessHelp();
			}
		}
		} catch (Exception e) { 
			Console.WriteLine("Error processing "+line); 
            Console.WriteLine(e.ToString());
			Console.WriteLine(e.StackTrace); 
		}
		return true;
	}

	private static void RunQuery(String Line)  {
        bool showTypes = false; 

        string? envVar = Environment.GetEnvironmentVariable("MAPEPIRE_SHOW_TYPES");
		if (envVar != null) showTypes=true; 

		int runQueryIndex = Line.ToUpper().IndexOf("RUNQUERY"); 

		Line = Line.Substring(runQueryIndex+8);
		// Initialize and execute query
        
        if (job == null ) { 
            Console.WriteLine("Job is null"); return; 
        }
		
		Query query = job.Query(Line);
		QueryResult result = query.Execute();

		String? errorString = result.Error;
		if (errorString != null) { 
			Console.WriteLine("Query returned errorString:"+errorString); 
		} else { 
			QueryMetadata? metadata = result.Metadata ?? throw new Exception("NULL metadata");
                
            List<Dictionary<String,Object>>? data = result.Data;
			if (data == null) throw new Exception("NULL data"); 
			Console.WriteLine(" rows returned = "+data.Count+" isDone="+result.IsDone);
            List<ColumnMetadata>? columns = metadata.Columns ?? throw new Exception("NULL column");
                
			columns.ForEach(col=>Console.Write(col.Name+" ")); 
			Console.WriteLine(); 
			

			List<Dictionary<String,Object>>.Enumerator enumerator = data.GetEnumerator(); 
			while (enumerator.MoveNext()) { 
				Dictionary<String, Object> hashmap = (Dictionary <String,Object>) enumerator.Current; 
				columns.ForEach(col=>Console.Write(hashmap.GetValueOrDefault(col.Name ?? throw new Exception("Null column"))+" "));
                Console.WriteLine(); 
				if (showTypes) {
				   List<ColumnMetadata>.Enumerator columnEnumerator = columns.GetEnumerator();
					while (columnEnumerator.MoveNext()) {
						Object? value = hashmap.GetValueOrDefault(columnEnumerator.Current.Name);
						if (value == null) { 
                           Console.Write("null "); 
						} else {
							Console.Write(value.GetType().ToString()+" "); 
						}
					}
                    Console.WriteLine(); 
				}
			}
			
		}
		// Close query and job
		query.Close();

		
		
	}

	private static void ProcessHelp() {
		Console.WriteLine("Possible commands"); 
		Console.WriteLine("connect host port user password validateCA CA");
		Console.WriteLine("runQuery QUERY");
		Console.WriteLine("help"); 
		Console.WriteLine("exit"); 
		
	}

	private static void connect(string[] lineElements) {

		int elementCount = lineElements.Length;
		string host  = "";
		int port = 0;
		string user = "";
		string password = "";
		bool rejectUnauthorized = true;
		string ca = "";

		if (elementCount > 1)
			host = lineElements[1];
		if (elementCount > 2)
			port = Int32.Parse(lineElements[2]);
		if (elementCount > 3)
			user = lineElements[3];
		if (elementCount > 4)
			password = lineElements[4];
		if (elementCount > 5)
			rejectUnauthorized = Boolean.Parse(lineElements[5]);
		if (elementCount > 6)
			ca = lineElements[6];
		DaemonServer newDaemonServer ;
		String successString; 
		if (elementCount <= 5) {
			newDaemonServer = new DaemonServer(host, port, user, password);
			successString  = "connection created using (" + host + "," + port + "," + user + ",*******)";
		} else if (elementCount == 6) {
			newDaemonServer = new DaemonServer(host, port, user, password, rejectUnauthorized, null);
			successString  = "connection created using (" + host + "," + port + "," + user + ",*******,"
					+ rejectUnauthorized + ")";
		} else {
			newDaemonServer = new DaemonServer(host, port, user, password, rejectUnauthorized, ca);
			successString  = "connection created using (" + host + "," + port + "," + user + ",*******,"
					+ rejectUnauthorized + "," + ca + ")";
		}
		daemonServer = newDaemonServer;
		SqlJob newJob = new SqlJob(); 
		ConnectionResult? cr = newJob.Connect(daemonServer);
		Console.WriteLine(successString);
        Console.WriteLine("JOB ="+newJob.Id);

		if (job != null) { 
			job.Close(); 
		}
		job = newJob; 
		 
         
	}

	private static string[] splitLine(String line) {
		return line.Split(" ");
	}

}
}