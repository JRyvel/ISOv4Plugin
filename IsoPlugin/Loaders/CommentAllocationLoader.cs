﻿using AgGateway.ADAPT.ApplicationDataModel.Notes;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.Plugins
{
    internal class CommentAllocationLoader
    {
        private TaskDataDocument _taskDocument;
        private List<Note> _allocations;

        private CommentAllocationLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _allocations = new List<Note>();
        }

        internal static List<Note> Load(XmlNode inputNode, TaskDataDocument taskDocument)
        {
            var loader = new CommentAllocationLoader(taskDocument);
            return loader.Load(inputNode.SelectNodes("CAN"));
        }

        private List<Note> Load(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                LoadCommentAllocations(inputNode);
            }

            return _allocations;
        }

        private void LoadCommentAllocations(XmlNode inputNode)
        {
            Note note = null;

            var commentId = inputNode.GetXmlNodeValue("@A");
            if (!string.IsNullOrEmpty(commentId))
                note = LoadCodedComment(inputNode, commentId);
            else
                note = new Note { Description = inputNode.GetXmlNodeValue("@C") };

            if (note == null)
                return;

            Point location;
            note.TimeStamp = AllocationTimestampLoader.Load(inputNode, out location);
            note.SpatialContext = location;

            _allocations.Add(note);
        }

        private Note LoadCodedComment(XmlNode inputNode, string commentId)
        {
            var comment = _taskDocument.Comments.FindById(commentId);
            if (comment == null)
                return null;

            var commentValueId = inputNode.GetXmlNodeValue("@B");
            if (string.IsNullOrEmpty(commentValueId))
                return null;

            var commentValue = comment.Values.FindById(commentValueId);
            if (commentValue == null)
                return null;

            return new Note
            {
                Value = new EnumeratedValue
                {
                    Value = commentValue,
                    Representation = comment.Comment
                }
            };
        }
    }
}
