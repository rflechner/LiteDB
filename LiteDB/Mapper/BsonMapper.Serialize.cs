using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LiteDB
{
    public partial class BsonMapper
    {
        /// <summary>
        /// Serialize a entity class to BsonDocument
        /// </summary>
        public virtual BsonDocument ToDocument(Type type, object entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            // if object is BsonDocument, just return them
            if (entity is BsonDocument) return (BsonDocument)(object)entity;

            return this.Serialize(type, entity, 0).AsDocument;
        }

        /// <summary>
        /// Serialize a entity class to BsonDocument
        /// </summary>
        public virtual BsonDocument ToDocument<T>(T entity)
        {
            return this.ToDocument(typeof(T), entity).AsDocument;
        }

        internal BsonValue Serialize(Type type, object obj, int depth)
        {
            if (++depth > MAX_DEPTH) throw LiteException.DocumentMaxDepth(MAX_DEPTH, type);

            switch (obj)
            {
                case null:
                    return BsonValue.Null;
                case BsonValue value:
                    return new BsonValue(value);
                case string v:
                    var str = TrimWhitespace ? v.Trim() : v;
                    if (EmptyStringToNull && str.Length == 0)
                        return BsonValue.Null;
                    else
                        return new BsonValue(str);
                case int v:
                    return new BsonValue(v);
                case long v:
                    return new BsonValue(v);
                case double v:
                    return new BsonValue(v);
                case decimal v:
                    return new BsonValue(v);
                case byte[] v:
                    return new BsonValue(v);
                case ObjectId v:
                    return new BsonValue(v);
                case Guid v:
                    return new BsonValue(v);
                case bool v:
                    return new BsonValue(v);
                case DateTime v:
                    return new BsonValue(v);
                case short _:
                case ushort _:
                case byte _:
                case sbyte _:
                    return new BsonValue(Convert.ToInt32(obj));
                case uint _:
                    return new BsonValue(Convert.ToInt64(obj));
                case ulong ulng:
                    var lng = unchecked((long)ulng);
                    return new BsonValue(lng);
                case float v:
                    return new BsonValue(Convert.ToDouble(v));
                case char _:
                case Enum _:
                    return new BsonValue(obj.ToString());
                default:
                    if (_customSerializer.TryGetValue(type, out var custom) || _customSerializer.TryGetValue(obj.GetType(), out custom))
                    {
                        return custom(obj);
                    }
                    // for dictionary
                    else if (obj is IDictionary dictionary)
                    {
                        // when you are converting Dictionary<string, object>
                        if (type == typeof(object))
                        {
                            type = obj.GetType();
                        }
#if NETFULL
                        var itemType = type.GetGenericArguments()[1];
#else
                var itemType = type.GetTypeInfo().GenericTypeArguments[1];
#endif
                        return SerializeDictionary(itemType, dictionary, depth);
                    }
                    // check if is a list or array
                    else if (obj is IEnumerable collection)
                    {
                        return SerializeArray(Reflection.GetListItemType(obj.GetType()), collection, depth);
                    }
                    // otherwise serialize as a plain object
                    else
                    {
                        return SerializeObject(type, obj, depth);
                    }
            }

            // if is already a bson value
        }

        private BsonArray SerializeArray(Type type, IEnumerable array, int depth)
        {
            var arr = new BsonArray();

            foreach (var item in array)
            {
                arr.Add(this.Serialize(type, item, depth));
            }

            return arr;
        }

        private BsonDocument SerializeDictionary(Type type, IDictionary dict, int depth)
        {
            var o = new BsonDocument();

            foreach (var key in dict.Keys)
            {
                var value = dict[key];

                o.RawValue[key.ToString()] = this.Serialize(type, value, depth);
            }

            return o;
        }

        private BsonDocument SerializeObject(Type type, object obj, int depth)
        {
            var o = new BsonDocument();
            var t = obj.GetType();
            var entity = this.GetEntityMapper(t);
            var dict = o.RawValue;

            // adding _type only where property Type is not same as object instance type
            if (type != t)
            {
                dict["_type"] = new BsonValue(t.FullName + ", " + t.GetTypeInfo().Assembly.GetName().Name);
            }

            foreach (var member in entity.Members.Where(x => x.Getter != null))
            {
                // get member value
                var value = member.Getter(obj);

                if (value == null && this.SerializeNullValues == false && member.FieldName != "_id") continue;

                // if member has a custom serialization, use it
                if (member.Serialize != null)
                {
                    dict[member.FieldName] = member.Serialize(value, this);
                }
                else
                {
                    dict[member.FieldName] = this.Serialize(member.DataType, value, depth);
                }
            }

            return o;
        }
    }
}