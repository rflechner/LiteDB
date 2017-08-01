﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDB
{
    internal class QueryAnd : Query
    {
        private Query _left;
        private Query _right;

        public QueryAnd(Query left, Query right)
            : base(null)
        {
            _left = left;
            _right = right;
        }

        internal override IEnumerable<IndexNode> ExecuteIndex(IndexService indexer, CollectionIndex index)
        {
            throw new NotSupportedException();
        }

        internal override bool ExecuteDocument(BsonDocument doc)
        {
            return _left.ExecuteDocument(doc) && _right.ExecuteDocument(doc);
        }

        internal override IEnumerable<IndexNode> Run(CollectionPage col, IndexService indexer)
        {
            var left = _left.Run(col, indexer);
            var right = _right.Run(col, indexer);

            // if any query (left/right) is FullScan, this query is full scan too
            this.Mode = _left.Mode == QueryMode.Document || _right.Mode == QueryMode.Document ? QueryMode.Document : QueryMode.Index;

            return left.Intersect(right, new IndexNodeComparer());
        }

        public override string ToString()
        {
            return string.Format("({0} and {1})", _left, _right);
        }
    }
}