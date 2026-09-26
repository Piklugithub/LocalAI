using LocalAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalAI.Domain.Entities
{
    public class LocalModel
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; }

        public string FilePath { get; private set; }

        public ModelFormat Format { get; private set; }

        public ModelCapability Capabilities { get; private set; }

        public long SizeInBytes { get; private set; }

        private LocalModel()
        {
            Name = string.Empty;
            FilePath = string.Empty;
        }

        public LocalModel(
            string name,
            string filePath,
            ModelFormat format,
            ModelCapability capabilities,
            long sizeInBytes)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Model name cannot be empty.",
                    nameof(name));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "Model file path cannot be empty.",
                    nameof(filePath));
            }

            if (sizeInBytes < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sizeInBytes),
                    "Model size cannot be negative.");
            }

            Id = Guid.NewGuid();
            Name = name.Trim();
            FilePath = filePath;
            Format = format;
            Capabilities = capabilities;
            SizeInBytes = sizeInBytes;
        }
    }
}
