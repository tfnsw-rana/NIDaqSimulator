using System;
using System.IO;
using NationalInstruments.DAQmx;
using System.Data;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace HelloWorld
{
    public class RuncDAQ
    {
        public RuncDAQ()
        {
            try
            {
                Console.WriteLine("Create Channel");
                NationalInstruments.DAQmx.Task Task1 = new NationalInstruments.DAQmx.Task();
                Task1.AIChannels.CreateStrainGageChannel("cDAQ9189-2049C4BMod1/ai0", "Strain0", -0.001, 0.001, AIStrainGageConfiguration.QuarterBridgeI, AIExcitationSource.Internal, 2, 2.08, 0, 120, 0.3, 0, AIStrainUnits.Strain);


                // Timing Settings
                Console.WriteLine("Configure Timing");
                Task1.Timing.ConfigureSampleClock("", 100, SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, 1000);

                Console.WriteLine("Verify the task");
                Task1.Control(TaskAction.Verify);

                //Console.WriteLine("Preparet the Table for Data");
                //InitializeDataTable(Task1.AIChannels, ref dataTable);
                //acquisitionDataGrid.DataSource = dataTable;

                //Logging Settings 
                Task1.ConfigureLogging("newlog", TdmsLoggingOperation.OpenOrCreate, LoggingMode.LogAndRead);


                //Offset Nulling 
                Task1.AIChannels[0].PerformBridgeOffsetNullingCalibration();

                //Shunt Calibration & Wheatstone Parameters
                //Task1.AIChannels[0].CalibrationEnable = true;
                //Task1.AIChannels[0].BridgeShuntCalibrationEnable = true;
                //myTask.AIChannels[0].PerformBridgeShuntCalibration(50000, ShuntElementLocation.R3, 100000);

                //Acquire Data
                Console.WriteLine("Read the data");
                AnalogSingleChannelReader reader1 = new AnalogSingleChannelReader(Task1.Stream);
                reader1.SynchronizeCallbacks = true;
                while (true)
                {                   
                    double sample = reader1.ReadSingleSample();
                    Console.WriteLine("Sample Captured: " + sample);
                }
                //Console.WriteLine("Write the data");
                //AnalogSingleChannelWriter writer1 = new AnalogSingleChannelWriter(Task1.Stream);


                //Console.WriteLine("Clear Task");
                //Task1.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                while (true)                     Console.WriteLine("Error");
            }

        }
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World Again");
            RuncDAQ mycDAQ = new RuncDAQ();
        }

        static void WriteBytes(double[] values)
        {
            var result = new byte[values.Length * sizeof(double)];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);
            FileStream output = new FileStream("test.bin", FileMode.Create, FileAccess.ReadWrite);
            BinaryWriter binWtr = new BinaryWriter(output);
            binWtr.Write(result);

        }
    }
}
