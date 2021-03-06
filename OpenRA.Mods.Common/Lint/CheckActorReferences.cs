#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Reflection;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Lint
{
	public class CheckActorReferences : ILintPass
	{
		Action<string> emitError;

		public void Run(Action<string> emitError, Action<string> emitWarning, Map map)
		{
			this.emitError = emitError;

			foreach (var actorInfo in map.Rules.Actors)
				foreach (var traitInfo in actorInfo.Value.Traits.WithInterface<ITraitInfo>())
					CheckTrait(actorInfo.Value, traitInfo, map);
		}

		void CheckTrait(ActorInfo actorInfo, ITraitInfo traitInfo, Map map)
		{
			var actualType = traitInfo.GetType();
			foreach (var field in actualType.GetFields())
			{
				if (field.HasAttribute<ActorReferenceAttribute>())
					CheckReference(actorInfo, traitInfo, field, map.Rules.Actors, "actor");
				if (field.HasAttribute<WeaponReferenceAttribute>())
					CheckReference(actorInfo, traitInfo, field, map.Rules.Weapons, "weapon");
				if (field.HasAttribute<VoiceSetReferenceAttribute>())
					CheckReference(actorInfo, traitInfo, field, map.Rules.Voices, "voice");
			}
		}

		void CheckReference<T>(ActorInfo actorInfo, ITraitInfo traitInfo, FieldInfo fieldInfo,
			IReadOnlyDictionary<string, T> dict, string type)
		{
			var values = LintExts.GetFieldValues(traitInfo, fieldInfo, emitError);
			foreach (var v in values)
				if (v != null && !dict.ContainsKey(v.ToLowerInvariant()))
					emitError("{0}.{1}.{2}: Missing {3} `{4}`."
						.F(actorInfo.Name, traitInfo.GetType().Name, fieldInfo.Name, type, v));
		}
	}
}