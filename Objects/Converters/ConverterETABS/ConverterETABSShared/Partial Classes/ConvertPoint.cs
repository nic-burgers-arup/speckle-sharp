﻿using System;
using Objects.Structural.Geometry;
using Objects.Geometry;
using ETABSv1;

namespace Objects.Converter.ETABS
{
    public partial class ConverterETABS
    {
        public void PointToNative(Node speckleStructNode)
        {
            throw new NotImplementedException(); 
        }
        public Node PointToSpeckle(string name)
        {
           
            var speckleStructNode = new Node();
            double x,y,z;
            x = y = z = 0;
            int v = Model.PointObj.GetCoordCartesian(name,ref x,ref y,ref z);
            speckleStructNode.basePoint = new Point();
            speckleStructNode.basePoint.x = x;
            speckleStructNode.basePoint.y = y;
            speckleStructNode.basePoint.z = z;
            speckleStructNode.name = name;

            bool[] restraints = null;
            v = Model.PointObj.GetRestraint(name, ref restraints);

            speckleStructNode.restraint = Restraint(restraints);


//TO DO: detach properties
            return speckleStructNode;
        }

    }
}