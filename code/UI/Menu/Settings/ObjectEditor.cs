
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace TFS2.UI;

public class ObjectEditor : Panel
{
	private const string EmptyGroupName = ClientSettings.OtherGroup;

	public ObjectEditor()
	{
		Style.FlexDirection = FlexDirection.Column;
	}

	public void SetTarget( object target )
	{
		DeleteChildren( true );

		var properties = TypeLibrary.GetPropertyDescriptions( target );

		//Sort all properties based on Display Info Group value
		Dictionary<string, List<PropertyDescription>> groupedProperties = new Dictionary<string, List<PropertyDescription>>();
		foreach ( var property in properties )
		{
			var displayInfo = property.GetDisplayInfo();

			if (!property.IsStatic)
			{
				//Use default group if empty
				string group = string.IsNullOrEmpty(displayInfo.Group) ? EmptyGroupName : displayInfo.Group;

				if (!groupedProperties.TryGetValue(group, out var groupPropertyList))
				{
					groupPropertyList = new List<PropertyDescription>();
					groupedProperties.Add(group, groupPropertyList);
				}

				groupPropertyList.Add(property);
			}
		}

		//Sort the grouped properties
		var groups = new List<KeyValuePair<string, List<PropertyDescription>>>(groupedProperties);
		groups.Sort((a, b) =>
		{
			int orderA = ClientSettings.GetGroupOrder(a.Key);
			int orderB = ClientSettings.GetGroupOrder(b.Key);

			return orderA.CompareTo(orderB);
        });

		//Loop over all groups
		foreach ( var propertyGroup in groups )
		{
			//Make heading
			Panel header = new Panel();

			header.SetClass( $"{propertyGroup.Key}", true);
			header.SetContent(propertyGroup.Key);

			AddChild(header);

			//Add settings row for properties
			foreach(var groupProperty in propertyGroup.Value)
			{
                AddChild(new SettingRow(target, groupProperty));
            }
        }
	}

}
