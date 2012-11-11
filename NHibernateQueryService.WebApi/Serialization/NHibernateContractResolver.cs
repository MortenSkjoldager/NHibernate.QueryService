﻿using System;
using System.Collections.Generic;
using System.Reflection;
using NHibernate.Proxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NHibernateQueryService.WebApi.Serialization {
    public class NHibernateContractResolver : DefaultContractResolver {
        //protected override JsonContract CreateContract(Type objectType) {
        //    if (typeof(NHibernate.Proxy.INHibernateProxy).IsAssignableFrom(objectType))
                
        //        return base.CreateContract(objectType.BaseType);
        //    else
        //        return base.CreateContract(objectType);
        //}

        private static readonly MemberInfo[] NHibernateProxyInterfaceMembers = typeof(INHibernateProxy).GetMembers();

        protected override List<MemberInfo> GetSerializableMembers(Type objectType) {
            var members = base.GetSerializableMembers(objectType);

            members.RemoveAll(memberInfo =>
                              (IsMemberPartOfNHibernateProxyInterface(memberInfo)) ||
                              (IsMemberDynamicProxyMixin(memberInfo)) ||
                              (IsMemberMarkedWithIgnoreAttribute(memberInfo, objectType)) ||
                              (IsMemberInheritedFromProxySuperclass(memberInfo, objectType)));

            return members;

            //var actualMemberInfos = new List<MemberInfo>();

            //foreach (var memberInfo in members) {
            //    var infos = memberInfo.DeclaringType.BaseType.GetMember(memberInfo.Name);
            //    actualMemberInfos.Add(infos.Length == 0 ? memberInfo : infos[0]);
            //}

            //return actualMemberInfos;
        }

        private static bool IsMemberDynamicProxyMixin(MemberInfo memberInfo) {
            return memberInfo.Name == "__interceptors";
        }

        private static bool IsMemberInheritedFromProxySuperclass(MemberInfo memberInfo, Type objectType) {
            return memberInfo.DeclaringType.Assembly == typeof(INHibernateProxy).Assembly;
        }

        private static bool IsMemberMarkedWithIgnoreAttribute(MemberInfo memberInfo, Type objectType) {
            var infos = typeof(INHibernateProxy).IsAssignableFrom(objectType)
                          ? objectType.BaseType.GetMember(memberInfo.Name)
                          : objectType.GetMember(memberInfo.Name);

            return infos[0].GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length > 0;
        }

        private static bool IsMemberPartOfNHibernateProxyInterface(MemberInfo memberInfo) {
            return Array.Exists(NHibernateProxyInterfaceMembers, mi => memberInfo.Name == mi.Name);
        }
    }
}