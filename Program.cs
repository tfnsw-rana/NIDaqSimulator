using System;
using System.IO;
using NationalInstruments.DAQmx;
using System.Data;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Timers;
using NationalInstruments;


namespace HelloWorld
{
    public class RuncDAQ
    {
        AnalogSingleChannelReader reader1;       
        double[] amplitudePeak;
        double[] locationPeak;
        double[] secondDerivatives;
        public RuncDAQ()
        {
            try
            {
                Console.WriteLine("Create Channel");
                NationalInstruments.DAQmx.Task Task1 = new NationalInstruments.DAQmx.Task();
                Task1.AIChannels.CreateStrainGageChannel("cDAQ9189-2049C4BMod1/ai0", "Strain0", -0.001, 0.001, AIStrainGageConfiguration.QuarterBridgeI, AIExcitationSource.Internal, 2, 2.08, 0, 120, 0.3, 0, AIStrainUnits.Strain);


                // Timing Settings
                // Check if you're using Onboard Clock as Sample Clock Source
                Console.WriteLine("Configure Timing");
                Task1.Timing.ConfigureSampleClock("", 100, SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, 1000);

                //Logging Settings 
                //Task1.ConfigureLogging("log5", TdmsLoggingOperation.Create, LoggingMode.LogAndRead);


                // Trigger Settings - Ensure Hardware Supports Trigger Type
                Console.WriteLine("Trigger Settings set to Default");
                //Task1.Triggers()

                Console.WriteLine("Verify the task");
                Task1.Control(TaskAction.Verify);

                //Offset Nulling 
                Console.WriteLine("Offset Nulling");
                Task1.AIChannels[0].PerformBridgeOffsetNullingCalibration();

                //Shunt Calibration & Wheatstone Parameters - Skip unsupported channels
                Console.WriteLine("Shunt Calibration & Wheatstone Parameters");               
                Task1.AIChannels[0].BridgeShuntCalibrationEnable = true;
                //Task1.AIChannels[0].CalibrationEnable = true;
                //Task1.AIChannels[0].CalibrationExpirationDate = DateTime.Today;
                //Task1.AIChannels[0].CalibrationApplyIfExpired = true;
                Task1.AIChannels[0].PerformBridgeShuntCalibration(50000, ShuntElementLocation.R3, 100000, true);


                //Console.WriteLine("Preparet the Table for Data");
                //InitializeDataTable(Task1.AIChannels, ref dataTable);
                //acquisitionDataGrid.DataSource = dataTable;

                //Start Task - Is this necessary?
                //Task1.Start();

                //Acquire Data
                Console.WriteLine("Configure reader");
                reader1 = new AnalogSingleChannelReader(Task1.Stream);
                reader1.SynchronizeCallbacks = true;

                //CounterSingleChannelReader counter1 = new CounterSingleChannelReader(Task1.Stream);

                Console.WriteLine("Configure writer ");
                AnalogSingleChannelWriter writer1 = new AnalogSingleChannelWriter(Task1.Stream);

                Console.WriteLine("Read 100 Samples");
                AnalogWaveform<double> data = reader1.ReadWaveform(1000);
                //writer1.WriteWaveform(true, data);
                for (int i = 0; i < 100; i++)
                {
                    Console.WriteLine(data.GetRawData()[i]);
                }
                
               

               Console.WriteLine("Clear Task");
                Task1.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //while (true)                     Console.WriteLine("Error");
            }

        }

        public void myCallBack(IAsyncResult ar)
        {
            double[] data = reader1.EndReadMultiSample(ar);            
            Console.WriteLine("Reading another 100");
            ar = reader1.BeginReadMultiSample(100, myCallBack, null);
        }

        static void Main(string[] args)
        {            
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
