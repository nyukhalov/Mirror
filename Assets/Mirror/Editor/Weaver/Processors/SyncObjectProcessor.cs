using System.Collections.Generic;
using Mono.CecilX;

namespace Mirror.Weaver
{
    public static class SyncObjectProcessor
    {
        // Finds SyncObjects fields in a type
        // Type should be a NetworkBehaviour
        public static List<FieldDefinition> FindSyncObjectsFields(Writers writers, Readers readers, Logger Log, TypeDefinition td)
        {
            List<FieldDefinition> syncObjects = new List<FieldDefinition>();

            foreach (FieldDefinition fd in td.Fields)
            {
                if (fd.FieldType.Resolve().ImplementsInterface<SyncObject>())
                {
                    if (fd.IsStatic)
                    {
                        Log.Error($"{fd.Name} cannot be static", fd);
                        Weaver.WeavingFailed = true;
                        continue;
                    }

                    GenerateReadersAndWriters(writers, readers, fd.FieldType);

                    syncObjects.Add(fd);
                }
            }


            return syncObjects;
        }

        // Generates serialization methods for synclists
        static void GenerateReadersAndWriters(Writers writers, Readers readers, TypeReference tr)
        {
            if (tr is GenericInstanceType genericInstance)
            {
                foreach (TypeReference argument in genericInstance.GenericArguments)
                {
                    if (!argument.IsGenericParameter)
                    {
                        readers.GetReadFunc(argument);
                        writers.GetWriteFunc(argument);
                    }
                }
            }

            if (tr != null)
            {
                GenerateReadersAndWriters(writers, readers, tr.Resolve().BaseType);
            }
        }
    }
}
