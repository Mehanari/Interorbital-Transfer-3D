using System;
using Newtonsoft.Json;

namespace MehaMath.Math.Components.Json
{
    /// <summary>
    /// Custom JsonConverter for the Vector struct to enable proper JSON serialization/deserialization.
    /// </summary>
    public class VectorJsonConverter : JsonConverter<Vector>
    {
        /// <summary>
        /// Reads the JSON representation of the Vector.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="hasExistingValue">True if there is an existing value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The deserialized Vector object.</returns>
        public override Vector ReadJson(JsonReader reader, Type objectType, Vector existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Handle null case
            if (reader.TokenType == JsonToken.Null)
                return new Vector(0);

            // Start reading the array of values
            if (reader.TokenType != JsonToken.StartArray)
                throw new JsonSerializationException("Expected start of array when deserializing Vector.");

            var values = new System.Collections.Generic.List<double>();
            
            reader.Read(); // Move to the first value or end of array
            
            // Read all values until the end of the array
            while (reader.TokenType != JsonToken.EndArray)
            {
                if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
                {
                    values.Add(Convert.ToDouble(reader.Value));
                }
                else
                {
                    throw new JsonSerializationException($"Unexpected token {reader.TokenType} when deserializing Vector.");
                }
                
                reader.Read(); // Move to the next token
            }
            
            // Create a new Vector with the values
            return new Vector(values.ToArray());
        }

        /// <summary>
        /// Writes the JSON representation of the Vector.
        /// </summary>
        /// <param name="writer">The JsonWriter to write to.</param>
        /// <param name="value">The Vector to write.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, Vector value, JsonSerializer serializer)
        {
            // Start writing as an array
            writer.WriteStartArray();

            // Write each value of the vector
            for (int i = 0; i < value.Length; i++)
            {
                writer.WriteValue(value[i]);
            }

            // End the array
            writer.WriteEndArray();
        }

        /// <summary>
        /// Gets a value indicating whether this converter can write JSON.
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// Gets a value indicating whether this converter can read JSON.
        /// </summary>
        public override bool CanRead => true;
    }
}