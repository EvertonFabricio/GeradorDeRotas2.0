﻿using MongoDB.Bson.Serialization.Attributes;

namespace Models
{
    public class Cidade
    {

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string Nome { get; set; }
    }
}
