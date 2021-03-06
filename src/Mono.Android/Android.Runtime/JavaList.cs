using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Android.Runtime;

using Java.Interop;

namespace Android.Runtime {

	[Register ("java/util/ArrayList", DoNotGenerateAcw=true)]
	// java.util.ArrayList allows null values
	public partial class JavaList : Java.Lang.Object, System.Collections.IList {

		internal static IntPtr arraylist_class = JNIEnv.FindClass ("java/util/List");
		internal static IntPtr id_add;
		static IntPtr id_clear;
		internal static IntPtr id_contains;
		internal static IntPtr id_get;
		internal static IntPtr id_indexOf;
		internal static IntPtr id_lastIndexOf;
		internal static IntPtr id_insert;
		static IntPtr id_iterator;
		static IntPtr id_remove;
		internal static IntPtr id_set;
		static IntPtr id_size;
		static IntPtr id_subList;

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.get(int)` throws an exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List.html?hl=en#get(int)
		//
		internal object? InternalGet (int location, Type? targetType = null)
		{
			if (id_get == IntPtr.Zero)
				id_get = JNIEnv.GetMethodID (arraylist_class, "get", "(I)Ljava/lang/Object;");

			IntPtr obj;
			try {
				obj = JNIEnv.CallObjectMethod (Handle, id_get, new JValue (location));
			} catch (Java.Lang.IndexOutOfBoundsException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
				throw new ArgumentOutOfRangeException (ex.Message, ex);
			}

			return JavaConvert.FromJniHandle (
					obj,
					JniHandleOwnership.TransferLocalRef,
					targetType);
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    No need to wrap thrown exceptions in a BCL class
		//
		//  Rationale
		//    `java.util.List.iterator()` is not documented to throw any exceptions.
		//
		public virtual Java.Util.IIterator Iterator ()
		{
			if (id_iterator == IntPtr.Zero)
				id_iterator = JNIEnv.GetMethodID (arraylist_class, "iterator", "()Ljava/util/Iterator;");
			return Java.Lang.Object.GetObject<Java.Util.IIterator> (
					JNIEnv.CallObjectMethod (Handle, id_iterator),
					JniHandleOwnership.TransferLocalRef)!;
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.set(int, Object)` throws a number of exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List.html?hl=en#set(int,%20E)
		//
		internal void InternalSet (int location, object? value)
		{
			if (id_set == IntPtr.Zero)
				id_set = JNIEnv.GetMethodID (arraylist_class, "set", "(ILjava/lang/Object;)Ljava/lang/Object;");
			IntPtr r = JavaConvert.WithLocalJniHandle (value, lref => {
				try {
					return JNIEnv.CallObjectMethod (Handle, id_set, new JValue (location), new JValue (lref));
				} catch (Java.Lang.UnsupportedOperationException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NotSupportedException (ex.Message, ex);
				} catch (Java.Lang.ClassCastException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new InvalidCastException (ex.Message, ex);
				} catch (Java.Lang.NullPointerException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NullReferenceException (ex.Message, ex);
				} catch (Java.Lang.IllegalArgumentException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new ArgumentException (ex.Message, ex);
				} catch (Java.Lang.IndexOutOfBoundsException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new ArgumentOutOfRangeException (ex.Message, ex);
				}
			});
			JNIEnv.DeleteLocalRef (r);
		}


		//
		// Exception audit:
		//
		//  Verdict
		//    No need to wrap thrown exceptions in a BCL class
		//
		//  Rationale
		//    `java.util.ArrayList.ctor()` is not documented to throw any exceptions. The `else` clause below
		//    instantiates a type we don't know at this time, so we have no information about the exceptions
		//    it may throw.
		//
		[Register (".ctor", "()V", "")]
		public JavaList ()
			: base (IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
		{
			if (Handle != IntPtr.Zero)
				return;

			if (GetType () == typeof (JavaList)) {
				SetHandle (
						JNIEnv.StartCreateInstance ("java/util/ArrayList", "()V"),
						JniHandleOwnership.TransferLocalRef);
			} else {
				SetHandle (
						JNIEnv.StartCreateInstance (GetType (), "()V"),
						JniHandleOwnership.TransferLocalRef);
			}
			JNIEnv.FinishCreateInstance (Handle, "()V");
		}

		public JavaList (IntPtr handle, JniHandleOwnership transfer) : base (handle, transfer) {}

		public JavaList (IEnumerable items) : this ()
		{
			if (items == null) {
				Dispose ();
				throw new ArgumentNullException ("items");
			}

			foreach (var item in items)
				Add (item);
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    No need to wrap thrown exceptions in a BCL class
		//
		//  Rationale
		//    `java.util.List.size()` is not documented to throw any exceptions.
		//
		public int Count {
			get {
				if (id_size == IntPtr.Zero)
					id_size = JNIEnv.GetMethodID (arraylist_class, "size", "()I");
				return JNIEnv.CallIntMethod (Handle, id_size);
			}
		}

		public bool IsFixedSize {
			get { return false; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public object? SyncRoot {
			get { return null; }
		}

		public object? this [int index] {
			get {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("index");
				return InternalGet (index);
			}
			set { InternalSet (index, value); }
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.add(Object)` throws a number of exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List?hl=en#add(E)
		//
		public int Add (object? item)
		{
			if (id_add == IntPtr.Zero)
				id_add = JNIEnv.GetMethodID (arraylist_class, "add", "(Ljava/lang/Object;)Z");
			JavaConvert.WithLocalJniHandle (item, lref => {
				try {
					return JNIEnv.CallBooleanMethod (Handle, id_add, new JValue (lref));
				} catch (Java.Lang.UnsupportedOperationException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NotSupportedException (ex.Message, ex);
				} catch (Java.Lang.ClassCastException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new InvalidCastException (ex.Message, ex);
				} catch (Java.Lang.NullPointerException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NullReferenceException (ex.Message, ex);
				} catch (Java.Lang.IllegalArgumentException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new ArgumentException (ex.Message, ex);
				}
			});
			return Count - 1;
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.clear()` throws an exception, see:
		//
		//     https://developer.android.com/reference/java/util/List?hl=en#clear()
		//
		public void Clear ()
		{
			if (id_clear == IntPtr.Zero)
				id_clear = JNIEnv.GetMethodID (arraylist_class, "clear", "()V");
			try {
				JNIEnv.CallVoidMethod (Handle, id_clear);
			} catch (Java.Lang.UnsupportedOperationException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
				throw new NotSupportedException (ex.Message, ex);
			}
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.contains(Object)` throws a number of exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List?hl=en#contains(java.lang.Object)
		//
		public bool Contains (object? item)
		{
			if (id_contains == IntPtr.Zero)
				id_contains = JNIEnv.GetMethodID (arraylist_class, "contains", "(Ljava/lang/Object;)Z");
			return JavaConvert.WithLocalJniHandle (item, lref => {
				try {
					return JNIEnv.CallBooleanMethod (Handle, id_contains, new JValue (lref));
				} catch (Java.Lang.ClassCastException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new InvalidCastException (ex.Message, ex);
				} catch (Java.Lang.NullPointerException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NullReferenceException (ex.Message, ex);
				}
			});
		}

		public void CopyTo (Array array, int array_index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (array_index < 0)
				throw new ArgumentOutOfRangeException ("array_index");
			if (array.Length < array_index + Count)
				throw new ArgumentException ("array");

			var targetType = array.GetType ().GetElementType ();
			int c = Count;
			for (int i = 0; i < c; i++)
				array.SetValue (InternalGet (i, targetType), array_index + i);
		}

		public IEnumerator GetEnumerator ()
		{
			return System.Linq.Extensions.ToEnumerator_Dispose (Iterator ());
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.indexOf(Object)` throws a number of exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List?hl=en#indexOf(java.lang.Object)
		//
		public virtual int IndexOf (object? item)
		{
			if (id_indexOf == IntPtr.Zero)
				id_indexOf = JNIEnv.GetMethodID (arraylist_class, "indexOf", "(Ljava/lang/Object;)I");
			return JavaConvert.WithLocalJniHandle (item, lref => {
				try {
					return JNIEnv.CallIntMethod (Handle, id_indexOf, new JValue (lref));
				} catch (Java.Lang.ClassCastException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new InvalidCastException (ex.Message, ex);
				} catch (Java.Lang.NullPointerException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NullReferenceException (ex.Message, ex);
				}
			});
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.lastIndexOf(Object)` throws a number of exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List?hl=en#lastIndexOf(java.lang.Object)
		//
		public virtual int LastIndexOf (object item)
		{
			if (id_lastIndexOf == IntPtr.Zero)
				id_lastIndexOf = JNIEnv.GetMethodID (arraylist_class, "lastIndexOf", "(Ljava/lang/Object;)I");
			return JavaConvert.WithLocalJniHandle (item, lref => {
				try {
					return JNIEnv.CallIntMethod (Handle, id_lastIndexOf, new JValue (lref));
				} catch (Java.Lang.ClassCastException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new InvalidCastException (ex.Message, ex);
				} catch (Java.Lang.NullPointerException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NullReferenceException (ex.Message, ex);
				}
			});
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.add(int, Object)` throws a number of exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List?hl=en#add(int,%20E)
		//
		public void Insert (int index, object? item)
		{
			if (id_insert == IntPtr.Zero)
				id_insert = JNIEnv.GetMethodID (arraylist_class, "add", "(ILjava/lang/Object;)V");

			JavaConvert.WithLocalJniHandle (item, lref => {
				try {
					JNIEnv.CallVoidMethod (Handle, id_insert, new JValue (index), new JValue (lref));
				} catch (Java.Lang.UnsupportedOperationException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NotSupportedException (ex.Message, ex);
				} catch (Java.Lang.ClassCastException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new InvalidCastException (ex.Message, ex);
				} catch (Java.Lang.NullPointerException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NullReferenceException (ex.Message, ex);
				} catch (Java.Lang.IllegalArgumentException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new ArgumentException (ex.Message, ex);
				} catch (Java.Lang.IndexOutOfBoundsException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new ArgumentOutOfRangeException (ex.Message, ex);
				}

				return IntPtr.Zero;
			});
		}

		public void Remove (object? item)
		{
			int i = IndexOf (item);
			if (i < 0 && i >= Count)
				return;
			RemoveAt (i);
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.remove(int)` throws a number of exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List?hl=en#remove(int)
		//
		public void RemoveAt (int index)
		{
			if (id_remove == IntPtr.Zero)
				id_remove = JNIEnv.GetMethodID (arraylist_class, "remove", "(I)Ljava/lang/Object;");
			try {
				IntPtr r = JNIEnv.CallObjectMethod (Handle, id_remove, new JValue (index));
				JNIEnv.DeleteLocalRef (r);
			} catch (Java.Lang.UnsupportedOperationException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
				throw new NotSupportedException (ex.Message, ex);
			} catch (Java.Lang.IndexOutOfBoundsException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
				throw new ArgumentOutOfRangeException (ex.Message, ex);
			}
		}
		
		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.set(int, Object)` throws a number of exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List?hl=en#set(int,%20E)
		//
		public virtual Java.Lang.Object? Set (int location, Java.Lang.Object item)
		{
			if (id_set == IntPtr.Zero)
				id_set = JNIEnv.GetMethodID (arraylist_class, "set", "(ILjava/lang/Object;)Ljava/lang/Object;");
			IntPtr obj;
			try {
				obj = JNIEnv.CallObjectMethod (Handle, id_set, new JValue (location), new JValue (item));
			} catch (Java.Lang.UnsupportedOperationException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
				throw new NotSupportedException (ex.Message, ex);
			} catch (Java.Lang.ClassCastException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
				throw new InvalidCastException (ex.Message, ex);
			} catch (Java.Lang.NullPointerException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
				throw new NullReferenceException (ex.Message, ex);
			} catch (Java.Lang.IllegalArgumentException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
				throw new ArgumentException (ex.Message, ex);
			} catch (Java.Lang.IndexOutOfBoundsException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
				throw new ArgumentOutOfRangeException (ex.Message, ex);
			}
			return Java.Lang.Object.GetObject<Java.Lang.Object> (
					obj,
					JniHandleOwnership.TransferLocalRef);
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.subList(int, int)` throws a number of exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List?hl=en#subList(int,%20int)
		//     https://developer.android.com/reference/java/util/ArrayList?hl=en#subList(int,%20int)
		//
		public virtual JavaList SubList (int start, int end)
		{
			if (id_subList == IntPtr.Zero)
				id_subList = JNIEnv.GetMethodID (arraylist_class, "subList", "(II;)Ljava/util/List;");

			IntPtr obj;
			try {
				obj = JNIEnv.CallObjectMethod (Handle, id_subList, new JValue (start), new JValue (end));
			} catch (Java.Lang.IllegalArgumentException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
				throw new ArgumentException (ex.Message, ex);
			} catch (Java.Lang.IndexOutOfBoundsException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
				throw new ArgumentOutOfRangeException (ex.Message, ex);
			}
			return new JavaList (
					obj,
					JniHandleOwnership.TransferLocalRef);
		}
		
		[Preserve (Conditional=true)]
		public static IList? FromJniHandle (IntPtr handle, JniHandleOwnership transfer)
		{
			if (handle == IntPtr.Zero)
				return null;

			var inst = (IJavaObject?) Java.Lang.Object.PeekObject (handle);
			if (inst == null)
				inst = new JavaList (handle, transfer);
			else
				JNIEnv.DeleteRef (handle, transfer);

			return (IList) inst;
		}

		[Preserve (Conditional=true)]
		public static IntPtr ToLocalJniHandle (IList? items)
		{
			if (items == null)
				return IntPtr.Zero;

			var c = items as JavaList;
			if (c != null)
				return JNIEnv.ToLocalJniHandle (c);

			using (c = new JavaList (items))
				return JNIEnv.ToLocalJniHandle (c);
		}

		#region ArrayList "implementation"
		//
		// They provide "implementation" of java.util.ArrayList methods.
		// They could be "overriden" in derived classes if there is
		// some class that derives from ArrayList (since some methods
		// are abstract, it is mandatory to implement them.
		//
		// Here's the scenario that happens to any class that derives
		// from ArrayList and overrides add() method:
		//
		// - jar2xml excludes the add() override from the result api xml.
		//   In general, jar2xml excludes (AOSP excluded) any overrides
		//   that are defined enough in the base class. ArrayList has
		//   add() method, so the derived class does not need it.
		// - but in reality, it is not java.util.ArrayList but our
		//   JavaList which actually takes part of the base class.
		// - Since there is no API definition for add() because of above,
		//   they don't appear in the generated class.
		// - but after that, the generator tries to *implement* missing
		//   methods from its implementing interfaces, which includes
		//   java.util.List. And that interface contains add() method.
		//
		// Java.Util.IList does not exist, so we cannot implement explicitly.
		//	
		public virtual bool Add (Java.Lang.Object? item)
		{
			return Add (0, item);
		}

		public virtual bool Add (int index, Java.Lang.Object? item)
		{
			if (Contains (item))
				return false;
			Add ((object?) item);
			return true;
		}

		public virtual bool Add (JavaList collection)
		{
			return AddAll (0, collection);
		}
		
		public virtual bool AddAll (int location, JavaList collection)
		{
			int pos = location;
			bool ret = false;
			foreach (Java.Lang.Object? item in collection)
				ret |= Add (pos++, item);
			return ret;
		}
		
		// Clear() exists.
		
		public virtual bool Contains (Java.Lang.Object? item)
		{
			return Contains ((object?) item);
		}
		
		public virtual bool ContainsAll (JavaList collection)
		{
			foreach (Java.Lang.Object? item in collection)
				if (!Contains (item))
					return false;
			return true;
		}
		
		public virtual bool Equals (Java.Lang.Object obj)
		{
			var collection = obj as JavaList;
			if (collection == null || Count != collection.Count)
				return false;
			// I'm not sure if this should be valid (i.e. Count doesn't change), hopefully it can be premised...
			for (int i = 0; i < Count; i++)
				if (!(this [i]?.Equals (collection [i]) == true))
					return false;
			return true;
		}
		
		public virtual Java.Lang.Object? Get (int location)
		{
			return (Java.Lang.Object?) InternalGet (location);
		}

		public virtual int IndexOf (Java.Lang.Object? item)
		{
			return IndexOf ((object?) item);
		}

		public virtual bool IsEmpty {
			get { return Count == 0; }
		}
		
		// Iterate() exists.

		// LastIndexOf() exists (added above, for code style consistency).
		
		// ListIterator does not exist in MfA, so listIterator() methods cannot be implemented.
		
		public virtual Java.Lang.Object? Remove (int location)
		{
			var ret = Get (location);
			RemoveAt (location);
			return ret;
		}
		
		public virtual bool Remove (Java.Lang.Object? item)
		{
			int i = IndexOf (item);
			if (i < 0 && i >= Count)
				return false;
			RemoveAt (i);
			return true;
		}
		
		public virtual bool RemoveAll (JavaList collection)
		{
			bool ret = false;
			foreach (Java.Lang.Object? item in collection)
				ret |= Remove (item);
			return ret;
		}
		
		public virtual bool RetainAll (JavaList collection)
		{
			bool ret = false;
			for (int i = 0; i < Count; i++) {
				var item = Get (i);
				if (!collection.Contains (item!)) {
					Remove (item!);
					ret = true;
					i--;
				}
			}
			return ret;
		}
		
		// Set() exists (added above, for code style consistency).
		
		public virtual int Size ()
		{
			return Count;
		}
		
		// SubList() exists (added above, for code style consistency).
		
		public virtual Java.Lang.Object [] ToArray (Java.Lang.Object [] array)
		{
			if (array.Length < Count)
				array = new Java.Lang.Object [Count];
			CopyTo (array, 0);
			return array;
		}
		
		public virtual Java.Lang.Object [] ToArray ()
		{
			return ToArray (new Java.Lang.Object [0]);
		}
		#endregion
	}

	[Register ("java/util/ArrayList", DoNotGenerateAcw=true)]
	public class JavaList<T> : JavaList, IList<T> {

		//
		// Exception audit:
		//
		//  Verdict
		//    No need to wrap thrown exceptions in a BCL class
		//
		//  Rationale
		//    `java.util.ArrayList.ctor()` is not documented to throw any exceptions. The `else` clause below
		//    instantiates a type we don't know at this time, so we have no information about the exceptions
		//    it may throw.
		//
		[Register (".ctor", "()V", "")]
		public JavaList ()
			: base (IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
		{
			if (Handle != IntPtr.Zero)
				return;

			if (GetType () == typeof (JavaList<T>)) {
				SetHandle (
						JNIEnv.StartCreateInstance ("java/util/ArrayList", "()V"),
						JniHandleOwnership.TransferLocalRef);
			} else {
				SetHandle (
						JNIEnv.StartCreateInstance (GetType (), "()V"),
						JniHandleOwnership.TransferLocalRef);
			}
			JNIEnv.FinishCreateInstance (Handle, "()V");
		}

		public JavaList (IntPtr handle, JniHandleOwnership transfer)
			: base (handle, transfer)
		{
		}

		public JavaList (IEnumerable<T> items) : this ()
		{
			if (items == null) {
				Dispose ();
				throw new ArgumentNullException ("items");
			}

			foreach (T item in items)
				Add (item);
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.get(int)` throws an exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List.html?hl=en#get(int)
		//
		[return: MaybeNull]
		internal T InternalGet (int location)
		{
			if (id_get == IntPtr.Zero)
				id_get = JNIEnv.GetMethodID (arraylist_class, "get", "(I)Ljava/lang/Object;");

			IntPtr obj;
			try {
				obj = JNIEnv.CallObjectMethod (Handle, id_get, new JValue (location));
			} catch (Java.Lang.IndexOutOfBoundsException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
				throw new ArgumentOutOfRangeException (ex.Message, ex);
			}

			return JavaConvert.FromJniHandle<T> (
					obj,
					JniHandleOwnership.TransferLocalRef);
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.set(int, Object)` throws a number of exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List.html?hl=en#set(int,%20E)
		//
		internal void InternalSet (int location, T value)
		{
			if (id_set == IntPtr.Zero)
				id_set = JNIEnv.GetMethodID (arraylist_class, "set", "(ILjava/lang/Object;)Ljava/lang/Object;");
			IntPtr r = JavaConvert.WithLocalJniHandle (value, lref => {
				try {
					return JNIEnv.CallObjectMethod (Handle, id_set, new JValue (location), new JValue (lref));
				} catch (Java.Lang.UnsupportedOperationException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NotSupportedException (ex.Message, ex);
				} catch (Java.Lang.ClassCastException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new InvalidCastException (ex.Message, ex);
				} catch (Java.Lang.NullPointerException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NullReferenceException (ex.Message, ex);
				} catch (Java.Lang.IllegalArgumentException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new ArgumentException (ex.Message, ex);
				} catch (Java.Lang.IndexOutOfBoundsException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new ArgumentOutOfRangeException (ex.Message, ex);
				}
			});
			JNIEnv.DeleteLocalRef (r);
		}

		// C#'s IList<T> allows nulls but is not annotated as MaybeNull.
		[MaybeNull]
		public T this [int index] {
			[return: MaybeNull]
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member because of nullability attributes.
			get {
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member because of nullability attributes.
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("index");
				return InternalGet (index);
			}
			set { InternalSet (index, value); }
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.add(Object)` throws a number of exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List?hl=en#add(E)
		//
		public void Add (T item)
		{
			if (id_add == IntPtr.Zero)
				id_add = JNIEnv.GetMethodID (arraylist_class, "add", "(Ljava/lang/Object;)Z");
			JavaConvert.WithLocalJniHandle (item, lref => {
				try {
					return JNIEnv.CallBooleanMethod (Handle, id_add, new JValue (lref));
				} catch (Java.Lang.UnsupportedOperationException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NotSupportedException (ex.Message, ex);
				} catch (Java.Lang.ClassCastException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new InvalidCastException (ex.Message, ex);
				} catch (Java.Lang.NullPointerException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NullReferenceException (ex.Message, ex);
				} catch (Java.Lang.IllegalArgumentException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new ArgumentException (ex.Message, ex);
				}
			});
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.contains(Object)` throws a number of exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List?hl=en#contains(java.lang.Object)
		//
		public bool Contains (T item)
		{
			if (id_contains == IntPtr.Zero)
				id_contains = JNIEnv.GetMethodID (arraylist_class, "contains", "(Ljava/lang/Object;)Z");
			return JavaConvert.WithLocalJniHandle (item, lref => {
				try {
					return JNIEnv.CallBooleanMethod (Handle, id_contains, new JValue (lref));
				} catch (Java.Lang.ClassCastException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new InvalidCastException (ex.Message, ex);
				} catch (Java.Lang.NullPointerException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NullReferenceException (ex.Message, ex);
				}
			});
		}

		public void CopyTo (T[] array, int array_index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (array_index < 0)
				throw new ArgumentOutOfRangeException ("array_index");
			if (array.Length < array_index + Count)
				throw new ArgumentException ("array");

			for (int i = 0; i < Count; i++)
#pragma warning disable CS8601 // Possible null reference assignment.
				array [array_index + i] = this [i];
#pragma warning restore CS8601 // Possible null reference assignment.
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ()!;
		}

		[return: MaybeNull]
		public IEnumerator<T> GetEnumerator ()
		{
			return System.Linq.Extensions.ToEnumerator_Dispose<T> (Iterator ());
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.indexOf(Object)` throws a number of exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List?hl=en#indexOf(java.lang.Object)
		//
		public int IndexOf (T item)
		{
			if (id_indexOf == IntPtr.Zero)
				id_indexOf = JNIEnv.GetMethodID (arraylist_class, "indexOf", "(Ljava/lang/Object;)I");
			return JavaConvert.WithLocalJniHandle (item, lref => {
				try {
					return JNIEnv.CallIntMethod (Handle, id_indexOf, new JValue (lref));
				} catch (Java.Lang.ClassCastException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new InvalidCastException (ex.Message, ex);
				} catch (Java.Lang.NullPointerException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NullReferenceException (ex.Message, ex);
				}
			});
		}

		//
		// Exception audit:
		//
		//  Verdict
		//    Exception wrapping is required.
		//
		//  Rationale
		//    `java.util.List.add(int, Object)` throws a number of exceptions, see:
		//
		//     https://developer.android.com/reference/java/util/List?hl=en#add(int,%20E)
		//
		public void Insert (int index, T item)
		{
			if (id_insert == IntPtr.Zero)
				id_insert = JNIEnv.GetMethodID (arraylist_class, "add", "(ILjava/lang/Object;)V");

			JavaConvert.WithLocalJniHandle (item, lref => {
				try {
					JNIEnv.CallVoidMethod (Handle, id_insert, new JValue (index), new JValue (lref));
				} catch (Java.Lang.UnsupportedOperationException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NotSupportedException (ex.Message, ex);
				} catch (Java.Lang.ClassCastException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new InvalidCastException (ex.Message, ex);
				} catch (Java.Lang.NullPointerException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new NullReferenceException (ex.Message, ex);
				} catch (Java.Lang.IllegalArgumentException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new ArgumentException (ex.Message, ex);
				} catch (Java.Lang.IndexOutOfBoundsException ex) when (JNIEnv.ShouldWrapJavaException (ex)) {
					throw new ArgumentOutOfRangeException (ex.Message, ex);
				}
					return IntPtr.Zero;
			});
		}

		public bool Remove (T item)
		{
			int i = IndexOf (item);
			if (i < 0 && i >= Count)
				return false;
			RemoveAt (i);
			return true;
		}
		
		[Preserve (Conditional=true)]
		public static IList<T>? FromJniHandle (IntPtr handle, JniHandleOwnership transfer)
		{
			if (handle == IntPtr.Zero)
				return null;

			var inst = (IJavaObject?) Java.Lang.Object.PeekObject (handle, typeof (IList<T>));
			if (inst == null)
				inst = new JavaList<T> (handle, transfer);
			else
				JNIEnv.DeleteRef (handle, transfer);

			return (IList<T>) inst;
		}

		[Preserve (Conditional=true)]
		public static IntPtr ToLocalJniHandle (IList<T>? items)
		{
			if (items == null)
				return IntPtr.Zero;

			var c = items as JavaList<T>;
			if (c != null)
				return JNIEnv.ToLocalJniHandle (c);

			using (c = new JavaList<T>(items))
				return JNIEnv.ToLocalJniHandle (c);
		}
	}
}
